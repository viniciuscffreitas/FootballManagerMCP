using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Messages;

namespace FootballManager.Application.Services;

public interface INewsService
{
    /// <summary>The newest news about the user's club and his league, newest first, the text of each story on request.</summary>
    Task<Result<News>> GetNewsAsync(int? limit = null, bool includeBodies = false, CancellationToken cancellationToken = default);
}

public class NewsService(INewsGameClient gameClient) : INewsService
{
    public const int DefaultLimit = 10;
    public const int MaxLimit = 50;

    public async Task<Result<News>> GetNewsAsync(int? limit = null, bool includeBodies = false, CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > MaxLimit)
        {
            return Result.Fail<News>(new ValidationError("limit", [$"The limit runs 1 to {MaxLimit}."]));
        }

        var take = limit ?? DefaultLimit;

        try
        {
            var team = gameClient.GetNewsAsync(NewsFeed.Team, take, includeBodies, cancellationToken);
            var league = gameClient.GetNewsAsync(NewsFeed.League, take, includeBodies, cancellationToken);

            await Task.WhenAll(team, league);

            return Result.Ok(new News { Team = await team, League = await league });
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<News>(new GameUnavailableError(exception.Message));
        }
    }
}
