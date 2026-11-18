using FluentResults;
using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Services;

public interface IPlayerExitService
{
    /// <summary>
    /// Plans <paramref name="action"/> for one of the club's own players from his menu in the game, and sends it only
    /// when <paramref name="apply"/> is set, through the save guard. A menu with more than one entry needs
    /// <paramref name="choice"/>: one of the entries the dry run lists, as it labels them. No action is refused.
    /// </summary>
    Task<Result<PlayerExit>> ChangeAsync(
        int? playerId, PlayerExitAction? action, string? choice, bool apply, CancellationToken cancellationToken = default);
}
