using FootballManager.Domain.Messages;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the portal's news in the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when the game
/// can't be reached.
/// </summary>
public interface INewsGameClient
{
    /// <summary>
    /// The newest <paramref name="take"/> stories of the feed, newest first. A story's text comes only with
    /// <paramref name="includeBodies"/>.
    /// </summary>
    Task<NewsPage> GetNewsAsync(NewsFeed feed, int take, bool includeBodies, CancellationToken cancellationToken = default);
}
