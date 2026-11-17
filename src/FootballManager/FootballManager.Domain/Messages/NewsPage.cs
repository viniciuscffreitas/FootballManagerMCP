namespace FootballManager.Domain.Messages;

/// <summary>The newest stories of one news feed, newest first, as the game lists them.</summary>
public class NewsPage
{
    /// <summary>How many stories the feed holds.</summary>
    public int Total { get; set; }

    public List<NewsStory> Stories { get; set; } = [];
}
