using FootballManager.Domain.Board;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the board's and the supporters' view of the user from the running game. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when it can't.
/// </summary>
public interface IBoardGameClient
{
    Task<BoardStatus> GetBoardAsync(CancellationToken cancellationToken = default);
}
