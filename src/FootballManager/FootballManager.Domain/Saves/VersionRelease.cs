namespace FootballManager.Domain.Saves;

/// <summary>The game's version against the one cleared for writes on the real save, and whether it was just cleared.</summary>
public class VersionRelease
{
    public bool Applied { get; set; }

    /// <summary>The build the running game reports, e.g. "27.0.1.2494769".</summary>
    public required string CurrentVersion { get; set; }

    /// <summary>The build cleared before this call; null when none was.</summary>
    public string? ReleasedVersion { get; set; }

    public bool AlreadyReleased => string.Equals(CurrentVersion, ReleasedVersion, StringComparison.Ordinal);
}
