namespace FootballManager.Application.Settings;

/// <summary>The real save the guard protects, where the game keeps it, and where the copies go.</summary>
public class SaveGuardSettings
{
    /// <summary>The real save's file name without ".fm"; with none set, every write outside the test save is refused.</summary>
    public string? Save { get; set; }

    /// <summary>The club the real save's manager runs.</summary>
    public string? Club { get; set; }

    /// <summary>The game's saves folder (FM 27 on Windows).</summary>
    public string GamesFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Sports Interactive", "Football Manager 27", "games");

    /// <summary>Where the copies go, with the released-version file beside them.</summary>
    public string BackupFolder { get; set; } = @"C:\Code\fm-backup";

    /// <summary>
    /// The game build cleared for writes while the released-version file in <see cref="BackupFolder"/> doesn't exist;
    /// once it does, the file wins.
    /// </summary>
    public string? ReleasedVersion { get; set; }
}
