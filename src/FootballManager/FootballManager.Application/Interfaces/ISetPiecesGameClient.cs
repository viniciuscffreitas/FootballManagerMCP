using FootballManager.Domain.SetPieces;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// The captaincy and the set piece takers of the user's team in the running game. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game doesn't answer, and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write (e.g. the player's menu doesn't offer it).
/// </summary>
public interface ISetPiecesGameClient
{
    /// <summary>The captain, the vice, the stand-ins and every taker list, as the game holds them now.</summary>
    Task<SetPieceSetup> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>Gives the player the armband, as his menu's "Make captain" does.</summary>
    Task MakeCaptainAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Makes the player vice-captain, as his menu's "Make vice-captain" does.</summary>
    Task MakeViceCaptainAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Puts these players at the top of the taker list, in this order, inside the screen's edit session.</summary>
    Task SetTakersAsync(string list, IReadOnlyList<int> playerIds, CancellationToken cancellationToken = default);

    /// <summary>Has the staff pick every taker, as the screen's "ask the assistant" does.</summary>
    Task AskStaffToAssignTakersAsync(CancellationToken cancellationToken = default);
}
