namespace FootballManager.Api.Responses.Messages;

public class NewsResponse
{
    /// <summary>News about the user's club.</summary>
    public required NewsPageResponse Team { get; set; }

    /// <summary>News about the user's league.</summary>
    public required NewsPageResponse League { get; set; }
}
