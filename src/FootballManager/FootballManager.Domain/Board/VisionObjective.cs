namespace FootballManager.Domain.Board;

/// <summary>An objective of the club vision.</summary>
public class VisionObjective
{
    public required string Name { get; set; }

    /// <summary>When it's due, as the game words it, e.g. "Temporada atual" or "2030".</summary>
    public string? Year { get; set; }

    /// <summary>The game's progress number for it (6 seen; its scale isn't confirmed).</summary>
    public int? Progress { get; set; }

    /// <summary>The game's status code for it (0 seen on objectives in progress; other codes aren't confirmed).</summary>
    public int? Status { get; set; }
}
