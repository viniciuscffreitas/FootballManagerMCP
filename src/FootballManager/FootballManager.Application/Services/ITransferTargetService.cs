using FluentResults;
using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Services;

public interface ITransferTargetService
{
    /// <summary>
    /// Plans <paramref name="action"/> for a player of another club from his menu in the game (withdraw the offer made
    /// for him, mark or unmark him as a director of football's target) and sends it only when <paramref name="apply"/>
    /// is set, through the save guard. A missing player or action is refused before anything else.
    /// </summary>
    Task<Result<TransferTargetChange>> ChangeAsync(
        int? playerId, TransferTargetAction? action, bool apply, CancellationToken cancellationToken = default);
}
