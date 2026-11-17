using FootballManager.Domain.Availability;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads who can play the next match from the running game. Throws <see cref="Exceptions.GameUnavailableException"/>
/// when it can't.
/// </summary>
public interface IAvailabilityGameClient
{
    /// <summary>Every first-team player's current injury, the team's active bans, and the next match's squad rules.</summary>
    Task<AvailabilityReadout> GetAvailabilityAsync(CancellationToken cancellationToken = default);
}
