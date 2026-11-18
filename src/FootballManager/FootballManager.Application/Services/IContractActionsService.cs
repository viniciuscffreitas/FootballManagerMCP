using FluentResults;
using FootballManager.Domain.Contracts;

namespace FootballManager.Application.Services;

public interface IContractActionsService
{
    /// <summary>
    /// Lets one of the club's own players go, through his menu in the game: released on a free transfer (the club pays
    /// off his contract), his contract ended by mutual consent, or marked to leave when it ends. Only when
    /// <paramref name="apply"/> and <paramref name="confirmRelease"/> are both set: a release can't be undone.
    /// </summary>
    Task<Result<ContractChange>> ReleaseAsync(
        int playerId, ReleaseKind? how, bool apply, bool confirmRelease, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the squad status (the playing time his contract promises) of one of the club's own players to the
    /// status of his menu labelled <paramref name="status"/>. Only when <paramref name="apply"/> is set.
    /// </summary>
    Task<Result<ContractChange>> SetSquadStatusAsync(int playerId, string status, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hands the renewal of one of the club's own players' contracts to the director of football, through his menu.
    /// Only when <paramref name="apply"/> is set.
    /// </summary>
    Task<Result<ContractChange>> HandRenewalToDofAsync(int playerId, bool apply, CancellationToken cancellationToken = default);
}
