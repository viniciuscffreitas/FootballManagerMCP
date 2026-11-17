using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Transfers in the running game, through its own panels: the player's menu opens the transfer offer panel, whose fee
/// moves with its +/− buttons and goes with its "make offer" button. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game doesn't answer, and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface ITransferGameClient
{
    /// <summary>
    /// The player with that FM id where the user's scouting, a team's squad or the game's search has listed him, and
    /// whether his menu offers a transfer offer; null when none has.
    /// </summary>
    Task<TransferTarget?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Opens the transfer offer panel from the player's menu; the panel once it shows, or null if it didn't.</summary>
    Task<OfferPanel?> OpenOfferPanelAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Presses the fee's + (<paramref name="raise"/>) or − button once; the panel once the fee moved, or as it is if
    /// it didn't. Null if the panel closed.
    /// </summary>
    Task<OfferPanel?> StepFeeAsync(bool raise, CancellationToken cancellationToken = default);

    /// <summary>Presses the panel's "make offer" button; the panel after it, or null if it closed.</summary>
    Task<OfferPanel?> SendOfferAsync(CancellationToken cancellationToken = default);
}
