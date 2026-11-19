using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Registration;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

// Squad numbers go through the game's squad numbers screen: each player's dropdown offers the numbers, greying out those
// someone else wears; "Pedir ao auxiliar para atribuir" numbers everyone without one. Both end with the screen's
// confirmation ("Enviar a numeração atual do elenco?").
public class SquadNumbersService(IRegistrationGameClient game, ISaveGuard saveGuard) : ISquadNumbersService
{
    private static readonly TimeSpan SettlePause = TimeSpan.FromMilliseconds(500);

    public async Task<Result<SquadNumberChange>> ChangeNumberAsync(
        int? playerId, int? number, bool askAssistant, bool apply, CancellationToken cancellationToken = default)
    {
        if (askAssistant && (playerId is not null || number is not null))
        {
            return Fail("askAssistant", "Either ask the assistant to number everyone, or give one player a number: not both.");
        }

        if (!askAssistant && (playerId is null || number is null))
        {
            return Fail("number", "Give playerId and number, or askAssistant=true for the assistant to number everyone.");
        }

        try
        {
            return askAssistant
                ? await AssistantAsync(apply, cancellationToken)
                : await OneAsync(playerId!.Value, number!.Value, apply, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SquadNumberChange>(new GameUnavailableError(exception.Message));
        }
    }

    private async Task<Result<SquadNumberChange>> OneAsync(int playerId, int number, bool apply, CancellationToken cancellationToken)
    {
        if (await game.GetSquadNumberAsync(playerId, cancellationToken) is not { } player)
        {
            return Fail("playerId", $"{playerId} isn't in the first team (get_squad lists who is).");
        }

        var change = new SquadNumberChange { PlayerId = playerId, PlayerName = player.Name, From = player.Number, To = number };

        if (player.Number == number)
        {
            return Fail("number", $"{player.Name} already wears {number}.");
        }

        if (!player.CanChange)
        {
            return Fail("playerId", $"The game doesn't let {player.Name}'s number change now.");
        }

        if (player.Options.FirstOrDefault(option => option.Number == number) is not { } offered)
        {
            return Fail("number", $"The game doesn't offer {number} to {player.Name}: it offers {Range(player.Options)}.");
        }

        if (offered.Taken)
        {
            return Fail("number", $"{number} is taken ({offered.Label}): free it first or pick another.");
        }

        if (!apply)
        {
            return Result.Ok(change);
        }

        // Numbers left on the screen (by the user, or a change stopped midway) would go in with this confirm.
        if (await game.HasPendingNumberChangesAsync(cancellationToken))
        {
            return Fail("number", "The game's squad numbers screen holds numbers not confirmed: confirm or undo them in the game first. " +
                                  "Nothing was written.");
        }

        var before = await game.GetFirstTeamNumbersAsync(cancellationToken);

        if (await ClearAsync(change, cancellationToken) is { } refused)
        {
            return refused;
        }

        change.Write = await SendAsync(change, () => game.SetSquadNumberAsync(playerId, number, cancellationToken));
        change.Applied = change.Write != RegistrationWrite.NotWritten;

        if (change.Applied)
        {
            await Task.Delay(SettlePause, cancellationToken);

            try
            {
                var now = await game.GetSquadNumberAsync(playerId, cancellationToken);
                change.Confirmed = now?.Number == number;

                if (change.Confirmed == false)
                {
                    change.Warnings.Add($"Read back, {player.Name} wears {now?.Number?.ToString() ?? "no number"}, not {number}.");
                }

                // The confirm sends the whole screen: anyone else whose number moved went in with it.
                var after = (await game.GetFirstTeamNumbersAsync(cancellationToken)).ToDictionary(p => p.Id, p => p.SquadNumber);
                var moved = before.Where(p => p.Id != playerId && after.TryGetValue(p.Id, out var wears) && wears != p.SquadNumber).ToList();

                if (moved.Count > 0)
                {
                    change.Confirmed = false;
                    change.Warnings.Add("Other numbers changed with it: " + string.Join(", ", moved.Select(p =>
                        $"{p.Name} {p.SquadNumber?.ToString() ?? "none"} → {after[p.Id]?.ToString() ?? "none"}")) + ".");
                }
            }
            catch (GameUnavailableException exception)
            {
                change.Warnings.Add($"Sent, but the number couldn't be read back ({exception.Message}): check get_squad before sending it again.");
            }
        }

        return Result.Ok(change);
    }

    // The assistant's numbering is planned only: the screen's events for it (EventFmxAutoSelect and
    // EventFmxNumberAllPlayers, then its confirm) changed nothing in the test save (18/11/2026), so an apply is refused
    // before the save check.
    private async Task<Result<SquadNumberChange>> AssistantAsync(bool apply, CancellationToken cancellationToken)
    {
        if (apply)
        {
            return Fail("askAssistant", "This tool can't ask the assistant to number the squad yet: the game didn't take it in the test save. " +
                                        "Nothing was written; give each player a number instead.");
        }

        var change = new SquadNumberChange { AskAssistant = true, NeedingNumbersBefore = await game.CountPlayersNeedingNumbersAsync(cancellationToken) };
        change.Notes.Add("The assistant numbers the players without a number; the game may also renumber others. This tool can only plan it.");
        return Result.Ok(change);
    }

    private async Task<Result<SquadNumberChange>?> ClearAsync(SquadNumberChange change, CancellationToken cancellationToken)
    {
        var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

        if (clearance.IsFailed)
        {
            return Result.Fail<SquadNumberChange>(clearance.Errors);
        }

        change.Notes.Add(SaveNote(clearance.Value));
        return null;
    }

    private static async Task<RegistrationWrite> SendAsync(SquadNumberChange change, Func<Task> send)
    {
        try
        {
            await send();
            return RegistrationWrite.Written;
        }
        catch (GameRejectedException exception)
        {
            change.Warnings.Add($"The game refused it: {exception.Message}");
            return RegistrationWrite.NotWritten;
        }
        catch (GameUnavailableException exception)
        {
            // The send may have reached the game: the read back decides.
            change.Warnings.Add($"Sent, but the game didn't answer ({exception.Message}).");
            return RegistrationWrite.Unknown;
        }
    }

    private static string Range(List<SquadNumberOption> options)
    {
        return options.Count == 0 ? "none" : $"{options.Min(o => o.Number)} to {options.Max(o => o.Number)}";
    }

    private static Result<SquadNumberChange> Fail(string property, string message)
    {
        return Result.Fail<SquadNumberChange>(new ValidationError(property, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
