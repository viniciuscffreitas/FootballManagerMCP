using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the game's transfer activity screen. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.
/// </summary>
public interface ITransferActivityGameClient
{
    /// <summary>The screen's counts and values, and every player with an offer either way, with his offers.</summary>
    Task<TransferActivityReadout> GetTransferActivityAsync(CancellationToken cancellationToken = default);
}
