using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Board;

namespace FootballManager.Application.Services;

public interface IBoardService
{
    Task<Result<BoardStatus>> GetBoardAsync(CancellationToken cancellationToken = default);
}

public class BoardService(IBoardGameClient gameClient) : IBoardService
{
    public async Task<Result<BoardStatus>> GetBoardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetBoardAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<BoardStatus>(new GameUnavailableError(exception.Message));
        }
    }
}
