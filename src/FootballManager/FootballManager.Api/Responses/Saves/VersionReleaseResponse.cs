namespace FootballManager.Api.Responses.Saves;

public class VersionReleaseResponse
{
    public bool Applied { get; set; }

    /// <summary>The running game's build.</summary>
    public string CurrentVersion { get; set; } = "";

    /// <summary>The build cleared for writes on the real save before this call.</summary>
    public string? ReleasedVersion { get; set; }

    public bool AlreadyReleased { get; set; }
}
