using FootballManager.Domain.Youth;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the club's youth set-up and loans from the running game. Throws <see cref="Exceptions.GameUnavailableException"/>
/// when it can't.
/// </summary>
public interface IYouthGameClient
{
    /// <summary>The youth set-up, every player of the highest youth team and every player out on loan, as the game holds them.</summary>
    Task<YouthAndLoans> GetYouthAndLoansAsync(CancellationToken cancellationToken = default);
}
