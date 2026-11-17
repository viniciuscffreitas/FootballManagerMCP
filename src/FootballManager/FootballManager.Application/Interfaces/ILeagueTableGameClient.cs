using FootballManager.Domain.Standings;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the user's league table from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when
/// it can't.
/// </summary>
public interface ILeagueTableGameClient
{
    /// <summary>The user's league, team and press prediction, and the table of the league's current stage, if any.</summary>
    Task<LeagueTableReadout> GetLeagueTableAsync(CancellationToken cancellationToken = default);
}
