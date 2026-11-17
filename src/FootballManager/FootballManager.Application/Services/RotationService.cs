using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Availability;
using FootballManager.Domain.Rotation;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;

namespace FootballManager.Application.Services;

public interface IRotationService
{
    /// <summary>Plans the rotation of the active eleven; picks the reserves in the game only when <paramref name="apply"/> is set.</summary>
    Task<Result<RotationPlan>> SuggestAsync(bool apply, CancellationToken cancellationToken = default);
}

// Rotates the active tactic's eleven for the next match. A starter goes out when he can't play (injured, or banned) or
// is tired (condition under 85). A reserve comes in when he can play, has condition of at least 92 and is rated in the
// slot's role at its position (the game only rates players who can play there); a match-sharp one (75 or more) first,
// then the best rated. A tired starter only goes out for a reserve rated at most 2 below him; an unavailable one goes
// out for anyone who qualifies. Each slot keeps its position and role: the eleven goes to set_tactic in the order of
// the slots' index, so an apply passes the save guard there. Rotating again right after changes nobody: whoever came
// in is fit and available.
public class RotationService(
    IAvailabilityService availabilityService, ITacticEditorGameClient editor, ITacticsGameClient tactics,
    ITacticEditingService tacticEditing) : IRotationService
{
    public const int TiredBelow = 85;
    public const int FreshFrom = 92;
    public const int SharpFrom = 75;
    public const int MaxRatingDrop = 2;

    public async Task<Result<RotationPlan>> SuggestAsync(bool apply, CancellationToken cancellationToken = default)
    {
        var availability = await availabilityService.GetAvailabilityAsync(cancellationToken);

        if (availability.IsFailed)
        {
            return Result.Fail<RotationPlan>(availability.Errors);
        }

        EditableTactic tactic;
        var fits = new Dictionary<PitchPosition, RoleFit>();

        try
        {
            tactic = await editor.GetTacticAsync(cancellationToken);

            foreach (var position in tactic.Lineup.Select(slot => slot.Position.InPossession?.Position).OfType<PitchPosition>().Distinct())
            {
                fits[position] = await tactics.GetRoleFitAsync(position, inPossession: true, cancellationToken);
            }
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<RotationPlan>(new GameUnavailableError(exception.Message));
        }

        var plan = Plan(availability.Value, tactic, fits);

        if (plan.Swaps.Count == 0)
        {
            return Result.Ok(plan);
        }

        // set_tactic places changes by the whole eleven: with a slot empty there is no eleven to give it.
        if (tactic.Lineup.Any(slot => slot.PlayerId is null))
        {
            plan.Notes.Add("The eleven has an empty slot: fill it with set_tactic, then rotate again.");
            return Result.Ok(plan);
        }

        var edit = await tacticEditing.SetTacticAsync(Changes(tactic, plan.Swaps), apply, cancellationToken);

        if (edit.IsFailed)
        {
            return Result.Fail<RotationPlan>(edit.Errors);
        }

        plan.Edit = edit.Value;
        plan.Applied = edit.Value.Applied;
        return Result.Ok(plan);
    }

    private static RotationPlan Plan(MatchAvailability availability, EditableTactic tactic, Dictionary<PitchPosition, RoleFit> fits)
    {
        var plan = new RotationPlan { Slot = tactic.ActiveSlot, NextMatch = availability.NextMatch };
        var players = availability.Players.ToDictionary(player => player.Id);
        var taken = tactic.Lineup.Select(slot => slot.PlayerId).OfType<int>().ToHashSet();

        foreach (var slot in tactic.Lineup.OrderBy(slot => slot.Index))
        {
            if (slot.PlayerId is not { } starterId || !players.TryGetValue(starterId, out var starter)
                || Reason(starter, availability) is not { } reason)
            {
                continue;
            }

            var position = slot.Position.InPossession;
            var ratings = Ratings(position is null ? null : fits.GetValueOrDefault(position.Position), slot.Role.InPossession);
            var starterRating = ratings.GetValueOrDefault(starterId);
            // He has a reason to go out yet can play: only tiredness.
            var tired = starter.Available;

            var candidates = ratings
                .Where(rating => !taken.Contains(rating.Key) && players.TryGetValue(rating.Key, out var reserve)
                                 && reserve.Available && reserve.Condition >= FreshFrom)
                .Where(rating => !tired || starterRating is null || rating.Value.Max >= starterRating.Max - MaxRatingDrop)
                .Select(rating => (Player: players[rating.Key], Rating: rating.Value))
                .OrderByDescending(candidate => candidate.Player.MatchSharpness >= SharpFrom)
                .ThenByDescending(candidate => candidate.Rating.Max)
                .ThenByDescending(candidate => candidate.Rating.Min)
                .ToList();

            var swap = new RotationSwap
            {
                Index = slot.Index,
                Position = position?.Code,
                Role = slot.Role.InPossession,
                OutPlayerId = starterId,
                OutName = starter.Name,
                OutRating = starterRating,
                Reason = reason
            };

            if (candidates.Count == 0)
            {
                swap.Detail = tired
                    ? $"No available reserve with condition {FreshFrom}+ is rated at most {MaxRatingDrop} below him here: he stays."
                    : $"No available reserve with condition {FreshFrom}+ is rated here: he stays, so pick someone else for the slot.";
                plan.Unfilled.Add(swap);
                continue;
            }

            var (incoming, rating) = candidates[0];
            swap.InPlayerId = incoming.Id;
            swap.InName = incoming.Name;
            swap.InRating = rating;
            swap.InCondition = incoming.Condition;
            swap.InMatchSharpness = incoming.MatchSharpness;
            swap.Detail = incoming.MatchSharpness >= SharpFrom
                ? null
                : $"No reserve here has match sharpness {SharpFrom}+; he has {incoming.MatchSharpness?.ToString() ?? "none shown"}.";

            taken.Remove(starterId);
            taken.Add(incoming.Id);
            plan.Swaps.Add(swap);
        }

        // The game doesn't say who is registered for a competition, so the rules are only passed on.
        if (plan.Swaps.Count > 0 && availability.NextMatch.EligibilityRules.Count > 0)
        {
            plan.Notes.Add($"{availability.NextMatch.Competition ?? "The next match"} has registration rules " +
                           $"({string.Join("; ", availability.NextMatch.EligibilityRules)}). The game doesn't say who is " +
                           $"registered: check {string.Join(", ", plan.Swaps.Select(swap => swap.InName))} can play it.");
        }

        return plan;
    }

    // Null when he can play and isn't tired. A ban counts when its scope names the next match's competition; the game
    // words scopes its own way ("National league"), so one that names something else can't be told apart from a ban
    // elsewhere, and is treated as out.
    private static string? Reason(PlayerAvailability player, MatchAvailability availability)
    {
        if (player.Absences.Contains(Absence.Injured))
        {
            return "injured";
        }

        if (player.Absences.Contains(Absence.Suspended))
        {
            var competition = availability.NextMatch.Competition;
            var bans = availability.Suspended.Where(ban => ban.PlayerId == player.Id).ToList();
            var here = bans.FirstOrDefault(ban => competition is not null
                && string.Equals(ban.Scope?.Trim(), competition.Trim(), StringComparison.OrdinalIgnoreCase));

            if (here is not null)
            {
                return $"suspended in {competition}{(here.Reason is null ? "" : $" ({here.Reason})")}";
            }

            var scopes = string.Join(", ", bans.Select(ban => ban.Scope ?? "no scope given"));
            return $"suspended ({scopes}); can't tell whether that covers {competition ?? "the next match"}, so treated as out";
        }

        if (player.Condition < TiredBelow)
        {
            return $"tired: condition {player.Condition}, under {TiredBelow}";
        }

        return null;
    }

    // Each player's rating in the slot's role at its position; the position's first role when the slot's isn't listed.
    private static Dictionary<int, Rating> Ratings(RoleFit? fit, string? role)
    {
        var candidates = fit?.Roles.FirstOrDefault(r => r.Key == role) ?? fit?.Roles.FirstOrDefault();
        return candidates?.Players.ToDictionary(player => player.PlayerId, player => player.Rating) ?? [];
    }

    // All eleven in the order of the slots' index, each with its slot's position and role, so nothing moves.
    private static TacticChanges Changes(EditableTactic tactic, List<RotationSwap> swaps)
    {
        var incoming = swaps.ToDictionary(swap => swap.Index, swap => swap.InPlayerId!.Value);

        return new TacticChanges
        {
            Slot = tactic.ActiveSlot,
            Lineup = tactic.Lineup.OrderBy(slot => slot.Index).Select(slot => new LineupChange
            {
                PlayerId = incoming.TryGetValue(slot.Index, out var id) ? id : slot.PlayerId!.Value,
                Position = new ByPhase<string> { InPossession = slot.Position.InPossession?.Code, OutOfPossession = slot.Position.OutOfPossession?.Code },
                Role = new ByPhase<string> { InPossession = slot.Role.InPossession, OutOfPossession = slot.Role.OutOfPossession }
            }).ToList()
        };
    }
}
