using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Transfers in the running game, through its own panels: the player's menu opens the transfer offer panel, whose fee
/// moves with its +/− buttons and goes with its "make offer" button; the message of an accepted offer opens the contract
/// negotiation panel, which moves on with its own buttons. Throws
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

    /// <summary>The contract negotiation panel open in the game, or null when none is.</summary>
    Task<ContractPanel?> ReadContractPanelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the contract negotiation panel with the message's button at <paramref name="option"/> (one that opens it,
    /// NewsAction 7); the panel once it shows, or null if it didn't.
    /// </summary>
    Task<ContractPanel?> OpenContractPanelAsync(int messageId, int option, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the contract negotiation panel for one of the club's own players from his menu ("Iniciar negociação de
    /// contrato"); the panel once it shows, or null if it didn't.
    /// </summary>
    Task<ContractPanel?> OpenRenewalPanelAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Presses the panel's button for <paramref name="step"/>; the panel once it changed, or null if it closed.</summary>
    Task<ContractPanel?> PressAsync(ContractStep step, CancellationToken cancellationToken = default);
}
