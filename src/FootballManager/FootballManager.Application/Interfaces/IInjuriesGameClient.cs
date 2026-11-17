using FootballManager.Domain.Injuries;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the medical centre from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.
/// </summary>
public interface IInjuriesGameClient
{
    /// <summary>Every first-team player's injury history and risk, and the team's season injury summary.</summary>
    Task<MedicalCentre> GetMedicalCentreAsync(CancellationToken cancellationToken = default);
}
