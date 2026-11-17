namespace FootballManager.Domain.Messages;

/// <summary>A news story of the portal, as the game writes it (in its display language).</summary>
public class NewsStory
{
    public int Id { get; set; }

    /// <summary>"2026-03-15".</summary>
    public string? Date { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>What it's about, e.g. the club.</summary>
    public string? Subtitle { get; set; }

    /// <summary>The paper or site that published it.</summary>
    public string? Source { get; set; }

    /// <summary>The story's text, when asked for.</summary>
    public string? Body { get; set; }
}
