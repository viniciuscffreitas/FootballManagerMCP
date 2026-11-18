using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Registration;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

// Registration goes as the game's registration screen does it: a competition's players are ticked or unticked while its
// window is open, then the screen's Confirm sends the change. The game rechecks the competition's rules on every tick;
// the dry run works out the ones it can (squad size, goalkeepers) and says which it can't.
public class RegistrationService(IRegistrationGameClient game, ISaveGuard saveGuard) : IRegistrationService
{
    // The game's rule types (seen 18/11/2026 in the Brasileirão, the Copa do Brasil and the Sul-Americana).
    private const int MaxSquadSize = 9;
    private const int MinSquadSize = 165;
    private const int MinGoalkeepers = 87;

    // The game takes the change a frame or more after it's sent.
    private static readonly TimeSpan SettlePause = TimeSpan.FromMilliseconds(500);

    public async Task<Result<SquadRegistration>> GetRegistrationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await game.GetRegistrationAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SquadRegistration>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<RegistrationChange>> ChangeRegistrationAsync(
        string? competition, IReadOnlyList<int?>? register, IReadOnlyList<int?>? unregister, bool apply,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(competition))
        {
            return Fail("competition", "Name the competition, as get_squad_registration lists it.");
        }

        if (register?.Any(id => id is null) == true || unregister?.Any(id => id is null) == true)
        {
            return Fail("players", "Each player in register and unregister must be an FM id from get_squad_registration.");
        }

        var adds = (register ?? []).Select(id => id!.Value).Distinct().ToList();
        var removes = (unregister ?? []).Select(id => id!.Value).Distinct().ToList();

        if (adds.Count + removes.Count == 0)
        {
            return Fail("players", "Name at least one player to register or unregister.");
        }

        if (adds.Intersect(removes).ToList() is [var both, ..])
        {
            return Fail("players", $"{both} is both in register and in unregister.");
        }

        try
        {
            var registration = await game.GetRegistrationAsync(cancellationToken);

            if (Find(registration, competition) is not { } target)
            {
                var names = string.Join("; ", registration.Competitions.Select(c => $"'{c.Name}' ({c.Competition})"));
                return Fail("competition", $"No registration matches '{competition}'; the team has: {names}.");
            }

            var change = new RegistrationChange { Competition = target.Name };

            foreach (var id in adds)
            {
                if (target.Registered.FirstOrDefault(p => p.Id == id) is { } already)
                {
                    return Fail("register", $"{already.Name} ({id}) is already registered for {target.Name}.");
                }

                if (target.Unregistered.FirstOrDefault(p => p.Id == id) is not { } player)
                {
                    return Fail("register", $"{id} isn't a player the game can register for {target.Name} (get_squad_registration lists who is).");
                }

                change.Register.Add(player);
            }

            foreach (var id in removes)
            {
                if (target.Registered.FirstOrDefault(p => p.Id == id) is not { } player)
                {
                    return Fail("unregister", $"{id} isn't registered for {target.Name}.");
                }

                change.Unregister.Add(player);
            }

            if (!target.WindowOpen)
            {
                return Fail("competition", $"The registration window for {target.Name} is closed ({target.Window}): the game doesn't let it change now.");
            }

            foreach (var player in change.Register.Concat(change.Unregister))
            {
                if (!await game.CanChangeRegistrationAsync(target.Index, target.Name, player.Id, cancellationToken))
                {
                    return Fail("players", $"The game doesn't let {player.Name} ({player.Id}) be changed in {target.Name} now.");
                }
            }

            change.Rules = Check(target, change);
            change.Warnings.AddRange(change.Rules.Where(rule => rule.Broken == true)
                .Select(rule => $"The change breaks the rule '{rule.Rule}': {rule.After} against {rule.Limit}."));
            change.Notes.AddRange(change.Rules.Where(rule => rule.Broken is null)
                .Select(rule => $"The game checks the rule '{rule.Rule}' on confirming; this plan can't work it out."));

            if (!apply)
            {
                return Result.Ok(change);
            }

            if (change.Rules.Any(rule => rule.Broken == true))
            {
                return Fail("players", $"Nothing was written: {string.Join(" ", change.Warnings)}");
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<RegistrationChange>(clearance.Errors);
            }

            change.Notes.Add(SaveNote(clearance.Value));
            change.Applied = await WriteAsync(target, change, cancellationToken);
            await ReadBackAsync(target.Name, change, cancellationToken);
            return Result.Ok(change);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<RegistrationChange>(new GameUnavailableError(exception.Message));
        }
    }

    // Each tick, then the confirm. A write that fails stops the rest: what was already sent is reported per player.
    private async Task<bool> WriteAsync(CompetitionRegistration target, RegistrationChange change, CancellationToken cancellationToken)
    {
        var writes = change.Register.Select(p => (p, true)).Concat(change.Unregister.Select(p => (p, false))).ToList();

        foreach (var (player, _) in writes)
        {
            change.Writes[player.Id] = RegistrationWrite.NotWritten;
        }

        foreach (var (player, registered) in writes)
        {
            var state = await SendAsync(() => game.SetRegisteredAsync(target.Index, target.Name, player.Id, registered, cancellationToken));
            change.Writes[player.Id] = state.Write;

            if (state.Error is { } error)
            {
                change.Warnings.Add($"Stopped at {player.Name} ({player.Id}): {error}. The other players weren't sent; the screen's change isn't confirmed.");
                return change.Writes.Values.Any(write => write != RegistrationWrite.NotWritten);
            }
        }

        var confirm = await SendAsync(() => game.ConfirmRegistrationAsync(target.Index, target.Name, cancellationToken));
        change.Confirm = confirm.Write;

        if (confirm.Error is { } failure)
        {
            change.Warnings.Add($"The players were ticked but the confirm failed: {failure}.");
        }

        return true;
    }

    private static async Task<(RegistrationWrite Write, string? Error)> SendAsync(Func<Task> send)
    {
        try
        {
            await send();
            return (RegistrationWrite.Written, null);
        }
        catch (GameRejectedException exception)
        {
            return (RegistrationWrite.NotWritten, exception.Message);
        }
        catch (GameUnavailableException exception)
        {
            // The send may have reached the game: the read back decides.
            return (RegistrationWrite.Unknown, exception.Message);
        }
    }

    // The registration read again: every player asked must be where asked. A read the game doesn't answer leaves it
    // unconfirmed, never failed: the change may be in.
    private async Task ReadBackAsync(string competition, RegistrationChange change, CancellationToken cancellationToken)
    {
        await Task.Delay(SettlePause, cancellationToken);

        try
        {
            var now = Find(await game.GetRegistrationAsync(cancellationToken), competition);

            if (now is null)
            {
                change.Warnings.Add($"Sent, but {competition} no longer shows in the registrations read back: check get_squad_registration.");
                return;
            }

            var registered = now.Registered.Select(p => p.Id).ToHashSet();
            var wrong = change.Register.Where(p => !registered.Contains(p.Id))
                .Concat(change.Unregister.Where(p => registered.Contains(p.Id)))
                .ToList();

            change.Confirmed = wrong.Count == 0;

            if (wrong.Count > 0)
            {
                change.Warnings.Add($"Read back, not as asked: {string.Join(", ", wrong.Select(p => $"{p.Name} ({p.Id})"))}.");
            }
        }
        catch (GameUnavailableException exception)
        {
            change.Warnings.Add($"Sent, but the registration couldn't be read back ({exception.Message}): check get_squad_registration before sending it again.");
        }
    }

    // The registration by its name or its competition's, case aside; then one whose name holds the text.
    private static CompetitionRegistration? Find(SquadRegistration registration, string competition)
    {
        var wanted = competition.Trim();

        return registration.Competitions.FirstOrDefault(c => Same(c.Name, wanted) || Same(c.Competition, wanted))
               ?? registration.Competitions.SingleOrDefault(c => c.Name.Contains(wanted, StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool Same(string? a, string b)
    {
        return string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
    }

    private static List<RuleCheck> Check(CompetitionRegistration target, RegistrationChange change)
    {
        var sizeDelta = change.Register.Count - change.Unregister.Count;
        var keeperDelta = change.Register.Count(p => p.IsGoalkeeper) - change.Unregister.Count(p => p.IsGoalkeeper);

        return target.Rules.Select(rule =>
        {
            var check = new RuleCheck { Rule = rule.Text ?? rule.Title ?? rule.Type.ToString(CultureInfo.InvariantCulture), Before = rule.Value, Limit = Limit(rule) };
            int? delta = rule.Type switch
            {
                MaxSquadSize or MinSquadSize => sizeDelta,
                MinGoalkeepers => keeperDelta,
                _ => null
            };

            if (delta is { } d && rule.Value is { } before && check.Limit is { } limit)
            {
                check.After = before + d;
                check.Broken = rule.Type == MaxSquadSize ? check.After > limit : check.After < limit;
            }

            return check;
        }).ToList();
    }

    // The limit in the game's value, after the slash: "47/50 Máx." is 50.
    private static int? Limit(RegistrationRule rule)
    {
        var after = rule.Display?.Split('/') is [_, var tail, ..] ? new string(tail.TrimStart().TakeWhile(char.IsDigit).ToArray()) : "";
        return int.TryParse(after, NumberStyles.None, CultureInfo.InvariantCulture, out var limit) ? limit : null;
    }

    private static Result<RegistrationChange> Fail(string property, string message)
    {
        return Result.Fail<RegistrationChange>(new ValidationError(property, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
