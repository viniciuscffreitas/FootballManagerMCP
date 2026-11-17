namespace FootballManager.Api.Responses.Messages;

public class NewsStoryResponse
{
    public int Id { get; set; }
    public string? Date { get; set; }
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Source { get; set; }

    /// <summary>The story's text, when asked for.</summary>
    public string? Body { get; set; }
}
