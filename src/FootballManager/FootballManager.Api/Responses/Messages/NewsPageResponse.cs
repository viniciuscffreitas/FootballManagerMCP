namespace FootballManager.Api.Responses.Messages;

public class NewsPageResponse
{
    /// <summary>How many stories the feed holds.</summary>
    public int Total { get; set; }

    /// <summary>The stories listed here.</summary>
    public int Count { get; set; }

    public List<NewsStoryResponse> Stories { get; set; } = [];
}
