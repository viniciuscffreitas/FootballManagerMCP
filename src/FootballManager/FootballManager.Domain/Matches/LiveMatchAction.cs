namespace FootballManager.Domain.Matches;

/// <summary>A change to the match being played, or what a dry run says it would do.</summary>
public class LiveMatchAction
{
    public LiveControl Control { get; set; }

    /// <summary>False for a dry run, which writes nothing.</summary>
    public bool Applied { get; set; }

    /// <summary>What it changes, e.g. "Neymar off, Coutinho on (slot 9)".</summary>
    public string? Change { get; set; }

    /// <summary>The setting as the game held it before, and after the write as the game reads it back.</summary>
    public string? Before { get; set; }

    public string? After { get; set; }

    /// <summary>A team talk's lines on screen now, in the game's display language.</summary>
    public List<string> Lines { get; set; } = [];

    /// <summary>The line said, for a team talk.</summary>
    public string? Said { get; set; }

    /// <summary>What the panel showed after the line, for a team talk.</summary>
    public List<string> Reaction { get; set; } = [];

    /// <summary>The match read again after the write.</summary>
    public LiveMatch? Match { get; set; }

    public List<string> Notes { get; set; } = [];
}
