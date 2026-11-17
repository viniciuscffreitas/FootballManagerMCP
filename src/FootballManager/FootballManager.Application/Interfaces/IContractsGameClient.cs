using FootballManager.Domain.Contracts;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the first team's contracts from the running game. Throws <see cref="Exceptions.GameUnavailableException"/>
/// when it can't.
/// </summary>
public interface IContractsGameClient
{
    /// <summary>Every first-team player's contract, in the game's squad order.</summary>
    Task<List<SquadPlayerContract>> GetContractsAsync(CancellationToken cancellationToken = default);
}
