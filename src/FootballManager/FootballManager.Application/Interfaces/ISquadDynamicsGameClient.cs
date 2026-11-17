using FootballManager.Domain.DressingRoom;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the dressing room from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it
/// can't.
/// </summary>
public interface ISquadDynamicsGameClient
{
    /// <summary>The hierarchy, the atmosphere, the open happiness issues, the manager's promises and every first-team player's happiness.</summary>
    Task<SquadDynamics> GetSquadDynamicsAsync(CancellationToken cancellationToken = default);
}
