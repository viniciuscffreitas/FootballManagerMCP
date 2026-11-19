using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Text;
using FootballManager.Domain.Opposition;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

// The instructions against the next opponent, as the opposition instructions screen gives them: on each player of the
// lineup the game expects, tight marking, pressing, tackling and the foot to show him onto. A dry run reads the screen
// and plans the writes; nothing is written without apply, and the screen read back says what the game now holds.
public class OppositionInstructionsService(IOppositionInstructionsGameClient game, ISaveGuard saveGuard)
    : IOppositionInstructionsService
{
    // The game takes the change a frame or more after it's sent.
    private static readonly TimeSpan SettlePause = TimeSpan.FromMilliseconds(500);

    public async Task<Result<OppositionInstructionsChange>> ChangeAsync(
        OppositionInstructionsAction? action,
        string? player,
        IReadOnlyDictionary<OppositionInstruction, string>? settings,
        bool apply,
        CancellationToken cancellationToken = default)
    {
        // No default action: a request that leaves it out must not fall on the enum's first value.
        if (action is not { } requested)
        {
            return Fail("action", "Name the action: Set, Clear or ApplyStaffAdvice.");
        }

        try
        {
            if (await game.GetAsync(cancellationToken) is not { } current)
            {
                return Result.Fail<OppositionInstructionsChange>(new EntityDoesNotExistError());
            }

            var change = new OppositionInstructionsChange { Action = requested, Current = current };
            var planned = Plan(change, player, settings);

            if (planned.IsFailed)
            {
                return planned.ToResult<OppositionInstructionsChange>();
            }

            if (!apply)
            {
                return Result.Ok(change);
            }

            if (change.Plan.Count == 0)
            {
                change.Notes.Add("Nothing to write: the game already holds what was asked.");
                return Result.Ok(change);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<OppositionInstructionsChange>(clearance.Errors);
            }

            change.Notes.Add(SaveNote(clearance.Value));

            foreach (var write in change.Plan)
            {
                // Once one write is in, a later one failing must still report what went in, not a plain error.
                try
                {
                    await game.SetAsync(write.PlayerId, write.Instruction, write.To, cancellationToken);
                    change.Applied = true;
                }
                catch (Exception exception) when (change.Applied && exception is GameUnavailableException or GameRejectedException)
                {
                    change.Notes.Add($"Stopped at {write.Instruction} on {write.Name}: {exception.Message}.");
                    break;
                }
            }

            // The writes are in: a caller giving up now must not lose the report of what went in.
            await Task.Delay(SettlePause, CancellationToken.None);
            await ReadBackAsync(change, CancellationToken.None);
            return Result.Ok(change);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<OppositionInstructionsChange>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Fail("game", exception.Message);
        }
    }

    private static Result Plan(
        OppositionInstructionsChange change, string? player, IReadOnlyDictionary<OppositionInstruction, string>? settings)
    {
        var rows = change.Current.Players;

        if (change.Action == OppositionInstructionsAction.ApplyStaffAdvice)
        {
            OppositionPlayerInstructions? only = null;

            if (!string.IsNullOrWhiteSpace(player))
            {
                var found = Find(rows, player);

                if (found.IsFailed)
                {
                    return found.ToResult();
                }

                only = found.Value;
            }

            PlanAdvice(change, only);
            return Result.Ok();
        }

        if (string.IsNullOrWhiteSpace(player))
        {
            return Result.Fail(new ValidationError("player", [$"{change.Action} needs the opponent player, by FM id or name."]));
        }

        var target = Find(rows, player);

        if (target.IsFailed)
        {
            return target.ToResult();
        }

        if (change.Action == OppositionInstructionsAction.Clear)
        {
            foreach (var instruction in Enum.GetValues<OppositionInstruction>())
            {
                AddWrite(change, target.Value, instruction, OppositionInstructionValues.None);
            }

            return Result.Ok();
        }

        if (settings is not { Count: > 0 })
        {
            return Result.Fail(new ValidationError("instructions", ["Set needs at least one instruction: marking, pressing, tackling or showOntoFoot."]));
        }

        foreach (var (instruction, value) in settings)
        {
            var key = value.Trim().ToLowerInvariant();
            var allowed = OppositionInstructionValues.Of(instruction);

            if (!allowed.Contains(key))
            {
                return Result.Fail(new ValidationError(instruction.ToString(), [$"{instruction} takes {string.Join(", ", allowed)}; got '{value}'."]));
            }

            AddWrite(change, target.Value, instruction, key);
        }

        return Result.Ok();
    }

    // The staff's suggestions the game offers now (not greyed out), on players of the expected lineup. A suggestion of a
    // kind not mapped to an instruction is only listed: applying a guess could set the wrong instruction.
    private static void PlanAdvice(OppositionInstructionsChange change, OppositionPlayerInstructions? only)
    {
        foreach (var suggestion in change.Current.StaffSuggestions)
        {
            if (suggestion.IsDisabled || (only is not null && suggestion.PlayerId != only.PlayerId))
            {
                continue;
            }

            var row = change.Current.Players.FirstOrDefault(candidate => candidate.PlayerId == suggestion.PlayerId);

            if (suggestion is not { Instruction: { } instruction, Value: { } value } || row is null)
            {
                change.Notes.Add($"Not applied, its kind isn't known yet: \"{suggestion.Text}\" ({suggestion.Name}).");
                continue;
            }

            AddWrite(change, row, instruction, value);
        }
    }

    private static void AddWrite(
        OppositionInstructionsChange change, OppositionPlayerInstructions row, OppositionInstruction instruction, string value)
    {
        var from = row.Get(instruction);

        if (from == value || row.PlayerId is not { } id || change.Plan.Any(write => write.PlayerId == id && write.Instruction == instruction))
        {
            return;
        }

        change.Plan.Add(new OppositionInstructionWrite { PlayerId = id, Name = row.Name, Instruction = instruction, From = from, To = value });
    }

    // The writes are sent by now: a read back that fails must not report the apply as failed.
    private async Task ReadBackAsync(OppositionInstructionsChange change, CancellationToken cancellationToken)
    {
        try
        {
            change.After = await game.GetAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is GameUnavailableException or GameRejectedException or HttpRequestException)
        {
            change.Notes.Add($"Sent, but the instructions couldn't be read back ({exception.Message}): check them with a dry run before sending again.");
            return;
        }

        foreach (var write in change.Plan)
        {
            var row = change.After?.Players.FirstOrDefault(candidate => candidate.PlayerId == write.PlayerId);
            write.Confirmed = row?.Get(write.Instruction) == write.To;
        }

        if (change.Plan.Any(write => write.Confirmed == false))
        {
            change.Notes.Add("The read back doesn't show every change: see plan[].confirmed and after.");
        }
    }

    private static Result<OppositionPlayerInstructions> Find(List<OppositionPlayerInstructions> rows, string player)
    {
        var wanted = player.Trim();
        var matches = int.TryParse(wanted, out var id)
            ? rows.Where(row => row.PlayerId == id).ToList()
            : rows.Where(row => row.Name is not null && GameNames.Normalise(row.Name) == GameNames.Normalise(wanted)).ToList();

        if (matches.Count == 1)
        {
            return Result.Ok(matches[0]);
        }

        var lineup = string.Join(", ", rows.Select(row => $"{row.Name} ({row.PlayerId}, {row.Position?.Code})"));
        var problem = matches.Count == 0 ? $"'{wanted}' isn't in the lineup the game expects from the opponent" : $"'{wanted}' names several players";
        return Result.Fail(new ValidationError("player", [$"{problem}; it is: {lineup}."]));
    }

    private static Result<OppositionInstructionsChange> Fail(string property, string message)
    {
        return Result.Fail<OppositionInstructionsChange>(new ValidationError(property, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
