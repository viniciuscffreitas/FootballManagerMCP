namespace FootballManager.Domain.Messages;

/// <summary>The newest news about the user's club and about his league.</summary>
public class News
{
    public NewsPage Team { get; set; } = new();
    public NewsPage League { get; set; } = new();
}
