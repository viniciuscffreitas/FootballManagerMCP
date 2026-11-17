using FootballManager.Domain.Opposition;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the game's opposition reports from the running game. Throws <see cref="Exceptions.GameUnavailableException"/>
/// when it can't.
/// </summary>
public interface IOppositionGameClient
{
    /// <summary>The user's next opponent with the game's threat assessment; null when the team has no match ahead.</summary>
    Task<NextOpponent?> GetNextOpponentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The game's report on a team of the user's competitions, by its FM id (one of
    /// <see cref="ITeamsGameClient.GetTeamsAsync"/>'s); null for the user's next opponent. The threat level is left
    /// out: the game only assesses it for the next opponent (<see cref="GetNextOpponentAsync"/>).
    /// </summary>
    Task<OppositionReport> GetReportAsync(int? teamId, CancellationToken cancellationToken = default);
}
