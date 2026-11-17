using FootballManager.Domain.Staff;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the first team's staff from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it
/// can't.
/// </summary>
public interface IStaffGameClient
{
    /// <summary>Every first-team staff member with job, style, attributes and contract, and the club's staff counts.</summary>
    Task<TeamStaff> GetStaffAsync(CancellationToken cancellationToken = default);
}
