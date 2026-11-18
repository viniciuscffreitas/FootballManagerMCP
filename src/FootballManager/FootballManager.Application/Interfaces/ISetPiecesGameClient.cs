using FootballManager.Domain.SetPieces;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// The captaincy and the set piece takers of the user's team in the running game. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game doesn't answer, and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface ISetPiecesGameClient
{
    /// <summary>The captain, the vice, the stand-ins and every taker list, as the game holds them now.</summary>
    Task<SetPieceSetup> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>Whether the player's menu offers making him captain or vice now.</summary>
    Task<CaptaincyOffer> GetCaptaincyOfferAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Gives the player the armband, as his menu's "Tornar Capitão" does.</summary>
    Task MakeCaptainAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Makes the player vice-captain, as his menu's "Tornar Vice-capitão" does.</summary>
    Task MakeViceCaptainAsync(int playerId, CancellationToken cancellationToken = default);
}
