using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

public interface ILiveMatchControlService
{
    /// <summary>Brings a substitute on for a player on the pitch. A dry run unless <paramref name="apply"/>.</summary>
    Task<Result<LiveMatchAction>> SubstituteAsync(int playerOffId, int playerOnId, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Changes the mentality of the tactic the match is played with. A dry run unless <paramref name="apply"/>.</summary>
    Task<Result<LiveMatchAction>> SetMentalityAsync(string mentality, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Changes the role of a player on the pitch for one phase. A dry run unless <paramref name="apply"/>.</summary>
    Task<Result<LiveMatchAction>> SetRoleAsync(
        int playerId, bool inPossession, string role, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the team talk on screen (before kick-off, at half-time, at full time) and, with <paramref name="apply"/>,
    /// says the line at <paramref name="line"/> to the squad.
    /// </summary>
    Task<Result<LiveMatchAction>> TeamTalkAsync(int? line, bool apply, CancellationToken cancellationToken = default);
}

// Changes to the match while it's played go to the match's own tactic (a substitution) or the tactic the team plays,
// which the match follows (mentality, roles; seen on 16/11/2026: the mentality changed in 1.8 s with the match on).
// Each write is light: a few /get and one write, never the screen's UI tree while the ball rolls, since a long call
// stalls the match. The team talk is UI only, and it's on screen only while the match waits for it.
public class LiveMatchControlService(
    ILiveMatchGameClient live, ITacticEditorGameClient tactics, IMatchesService matches, ISaveGuard saveGuard)
    : ILiveMatchControlService
{
    public const string NoMatch = "No match is being played: these changes only work during a match.";

    // The Continue button's states while the team talk is on screen: before kick-off and at half-time, then at full time.
    public static readonly string[] TeamTalkStates = ["TeamTalk", "FullTimeTeamTalk"];

    public async Task<Result<LiveMatchAction>> SubstituteAsync(
        int playerOffId, int playerOnId, bool apply, CancellationToken cancellationToken = default)
    {
        return await RunAsync(async () =>
        {
            if (await live.GetSideAsync(cancellationToken) is not { } side)
            {
                return Refuse("match", NoMatch);
            }

            if (side.SubstitutionsRemaining is 0)
            {
                return Refuse("substitution", "The team has no substitutions left in this match.");
            }

            var places = await live.GetPlacesAsync(side.IsHome, cancellationToken);

            if (places.FirstOrDefault(place => place.PlayerId == playerOffId) is not { OnPitch: true } off)
            {
                return Refuse("playerOffId", $"Player {playerOffId} isn't on the pitch. On it now: {Names(places.Where(p => p.OnPitch))}.");
            }

            if (places.FirstOrDefault(place => place.PlayerId == playerOnId) is not { OnPitch: false } on)
            {
                return Refuse("playerOnId", $"Player {playerOnId} isn't on the bench. On it now: {Names(places.Where(p => !p.OnPitch))}.");
            }

            var action = new LiveMatchAction
            {
                Control = LiveControl.Substitution,
                Applied = apply,
                Change = $"{off.PlayerName} off, {on.PlayerName} on (place {off.Index})",
                Before = off.PlayerName
            };

            if (!apply)
            {
                action.Notes.Add("Applying swaps them in the match's tactic at once; the game makes the substitution at " +
                                 "the next stoppage, which uses one of the " +
                                 $"{side.SubstitutionsRemaining?.ToString() ?? "remaining"} substitutions.");
                return Result.Ok(action);
            }

            if (await PrepareAsync(action, cancellationToken) is { } refused)
            {
                return refused;
            }

            var now = await live.SubstituteAsync(side.IsHome, off, on, cancellationToken);
            action.After = now.PlayerName;

            if (now.PlayerId != on.PlayerId)
            {
                action.Notes.Add($"The match's tactic still has {now.PlayerName} at place {off.Index}: the game didn't take the substitution.");
            }
            else
            {
                action.Notes.Add("The game makes the substitution at the next stoppage; until then the match still shows " +
                                 $"{off.PlayerName} on the pitch.");
            }

            return await WithMatchAsync(action, cancellationToken);
        });
    }

    public async Task<Result<LiveMatchAction>> SetMentalityAsync(
        string mentality, bool apply, CancellationToken cancellationToken = default)
    {
        return await RunAsync(async () =>
        {
            if (await live.GetSideAsync(cancellationToken) is null)
            {
                return Refuse("match", NoMatch);
            }

            var keys = await live.GetMentalitiesAsync(cancellationToken);

            if (!keys.Contains(mentality))
            {
                return Refuse("mentality", $"The game has no mentality '{mentality}': one of {string.Join(", ", keys)}.");
            }

            var action = new LiveMatchAction { Control = LiveControl.Mentality, Applied = apply, Change = $"mentality to {mentality}" };

            if (!apply)
            {
                action.Notes.Add("Applying sets the mentality of the tactic the team plays; the match takes it at once.");
                return Result.Ok(action);
            }

            if (await PrepareAsync(action, cancellationToken) is { } refused)
            {
                return refused;
            }

            var outcome = await tactics.SetMentalityAsync(mentality, cancellationToken);
            (action.Before, action.After) = (outcome.Before, outcome.After);

            return await WithMatchAsync(action, cancellationToken);
        });
    }

    public async Task<Result<LiveMatchAction>> SetRoleAsync(
        int playerId, bool inPossession, string role, bool apply, CancellationToken cancellationToken = default)
    {
        return await RunAsync(async () =>
        {
            if (await live.GetSideAsync(cancellationToken) is not { } side)
            {
                return Refuse("match", NoMatch);
            }

            var roles = await live.GetRolesAsync(inPossession, cancellationToken);

            if (!roles.Contains(role))
            {
                return Refuse("role", $"The game has no {(inPossession ? "in" : "out of")} possession role '{role}': see get_tactic_options.");
            }

            var places = await live.GetPlacesAsync(side.IsHome, cancellationToken);

            if (places.FirstOrDefault(place => place.PlayerId == playerId) is not { OnPitch: true } place)
            {
                return Refuse("playerId", $"Player {playerId} isn't on the pitch. On it now: {Names(places.Where(p => p.OnPitch))}.");
            }

            // Roles are written to the tactic the team plays, by lineup place: only when it has him where the match does.
            if (await live.GetTacticPlayerAsync(place.Index, cancellationToken) != playerId)
            {
                return Refuse("playerId", $"The team's tactic doesn't have {place.PlayerName} at place {place.Index}, where the " +
                                          "match has him (after a substitution they can differ): nothing was written.");
            }

            var action = new LiveMatchAction
            {
                Control = LiveControl.Role,
                Applied = apply,
                Change = $"{place.PlayerName} (place {place.Index}) to {role} {(inPossession ? "in" : "out of")} possession"
            };

            if (!apply)
            {
                action.Notes.Add("Applying sets his role in the tactic the team plays, which the match follows.");
                return Result.Ok(action);
            }

            if (await PrepareAsync(action, cancellationToken) is { } refused)
            {
                return refused;
            }

            var outcome = await tactics.SetRoleAsync(place.Index, inPossession, role, cancellationToken);
            (action.Before, action.After) = (outcome.Before, outcome.After);

            return await WithMatchAsync(action, cancellationToken);
        });
    }

    public async Task<Result<LiveMatchAction>> TeamTalkAsync(int? line, bool apply, CancellationToken cancellationToken = default)
    {
        return await RunAsync(async () =>
        {
            if (line is < 0)
            {
                return Refuse("line", "A line is its index on the panel, from 0.");
            }

            if (apply && line is null)
            {
                return Refuse("line", "Name the line to say: its index on the panel, from 0 (a dry run lists them).");
            }

            // The panel is read from the UI tree, which is only done while the match waits for the talk.
            var state = await live.GetMatchDayStateAsync(cancellationToken);

            if (!TeamTalkStates.Contains(state))
            {
                return Refuse("match", $"The team talk isn't on screen (the match day is at {state ?? "no state"}): it shows " +
                                       "before kick-off, at half-time and at full time.");
            }

            if (await live.ReadTeamTalkAsync(cancellationToken) is not { } panel)
            {
                return Refuse("match", "The team talk panel isn't on screen.");
            }

            var action = new LiveMatchAction { Control = LiveControl.TeamTalk, Applied = apply, Lines = panel.Lines };

            if (line >= panel.Lines.Count)
            {
                return Refuse("line", $"The panel has no line {line} (0 to {panel.Lines.Count - 1}).");
            }

            action.Change = line is { } chosen ? $"say \"{panel.Lines[chosen]}\" to the squad" : null;

            if (!apply)
            {
                action.Notes.Add("Applying says the chosen line to the whole squad, as clicking it does.");
                return Result.Ok(action);
            }

            if (await PrepareAsync(action, cancellationToken) is { } refused)
            {
                return refused;
            }

            var after = await live.SayTeamTalkAsync(line!.Value, cancellationToken);
            action.Said = panel.Lines[line.Value];
            action.Lines = after?.Lines ?? [];
            action.Reaction = after?.Texts.Except(panel.Texts).ToList() ?? [];

            if (after is null)
            {
                action.Notes.Add("The panel closed after the line.");
            }

            return Result.Ok(action);
        });
    }

    private static async Task<Result<LiveMatchAction>> RunAsync(Func<Task<Result<LiveMatchAction>>> run)
    {
        try
        {
            return await run();
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<LiveMatchAction>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Refuse("game", exception.Message);
        }
    }

    // The match as get_live_match reads it, to show the change.
    private async Task<Result<LiveMatchAction>> WithMatchAsync(LiveMatchAction action, CancellationToken cancellationToken)
    {
        var match = await matches.GetLiveMatchAsync(cancellationToken);

        if (match.IsSuccess)
        {
            action.Match = match.Value;
        }
        else
        {
            action.Notes.Add("The match couldn't be read again after the write: " + string.Join(" ", match.Errors.Select(e => e.Message)));
        }

        return Result.Ok(action);
    }

    // The save guard's clearance as a note, or its refusal; nothing is written after a refusal.
    private async Task<Result<LiveMatchAction>?> PrepareAsync(LiveMatchAction action, CancellationToken cancellationToken)
    {
        var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

        if (clearance.IsFailed)
        {
            return Result.Fail<LiveMatchAction>(clearance.Errors);
        }

        action.Notes.Add(SaveNote(clearance.Value));
        return null;
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }

    private static string Names(IEnumerable<MatchPlace> places)
    {
        var named = places.Where(place => place.PlayerId is not null).Select(place => $"{place.PlayerName} ({place.PlayerId})").ToList();
        return named.Count > 0 ? string.Join(", ", named) : "nobody";
    }

    private static Result<LiveMatchAction> Refuse(string field, string message)
    {
        return Result.Fail<LiveMatchAction>(new ValidationError(field, [message]));
    }
}
