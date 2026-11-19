using FluentResults;
using FootballManager.Domain.SetPieces;

namespace FootballManager.Application.Services;

public interface ISetPiecesService
{
    /// <summary>The captain, the vice, the armband's stand-ins and every taker list, read live.</summary>
    Task<Result<SetPieceSetup>> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes one player captain and/or another vice-captain. A dry run unless <paramref name="apply"/>; a player who
    /// already holds the role is left as he is.
    /// </summary>
    Task<Result<SetPieceEdit>> SetCaptainsAsync(int? captainId, int? viceCaptainId, bool apply, CancellationToken cancellationToken = default);
}
