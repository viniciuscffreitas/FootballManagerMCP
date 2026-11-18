using FootballManager.Domain.Conversations;
using FootballManager.Domain.Matches;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Changes the user's match while it's being played, the way the match screen does. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game can't answer and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface ILiveMatchGameClient
{
    /// <summary>The user's side in the match being played, or null when no match is being played.</summary>
    Task<LiveSide?> GetSideAsync(CancellationToken cancellationToken = default);

    /// <summary>The places of the user's matchday squad in the match's tactic, the eleven then the bench.</summary>
    Task<List<MatchPlace>> GetPlacesAsync(bool isHome, CancellationToken cancellationToken = default);

    /// <summary>
    /// Swaps a substitute in for a player on the pitch in the match's tactic, as dragging him onto the player does.
    /// The game makes the substitution at the next stoppage. Returns the place on the pitch as the tactic holds it after.
    /// </summary>
    Task<MatchPlace> SubstituteAsync(bool isHome, MatchPlace off, MatchPlace on, CancellationToken cancellationToken = default);

    /// <summary>The player at a lineup place of the tactic the team plays (the one tactic writes go to).</summary>
    Task<int?> GetTacticPlayerAsync(int index, CancellationToken cancellationToken = default);

    /// <summary>The game's mentality keys, e.g. Positive.</summary>
    Task<List<string>> GetMentalitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>The game's role keys for a phase, e.g. InsideForward.</summary>
    Task<List<string>> GetRolesAsync(bool inPossession, CancellationToken cancellationToken = default);

    /// <summary>Where the match day stands, as the Continue button's state names it, e.g. TeamTalk.</summary>
    Task<string?> GetMatchDayStateAsync(CancellationToken cancellationToken = default);

    /// <summary>The team talk panel on screen, or null when none is.</summary>
    Task<ConversationCard?> ReadTeamTalkAsync(CancellationToken cancellationToken = default);

    /// <summary>Says the team talk's line at <paramref name="line"/>; returns the panel after, null once it closed.</summary>
    Task<ConversationCard?> SayTeamTalkAsync(int line, CancellationToken cancellationToken = default);
}
