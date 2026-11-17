namespace FootballManager.Domain.Saves;

/// <summary>A copy of a save in the backup folder, with what its file name tells.</summary>
public class SaveBackup
{
    /// <summary>The file name, e.g. "Santos-20261117-1430-jogo-2026-03-10.fm".</summary>
    public required string Name { get; set; }

    public SaveBackupKind Kind { get; set; }

    /// <summary>The club the name starts with; null for a file named another way.</summary>
    public string? Club { get; set; }

    /// <summary>When the copy was made (local time, to the minute); null for a file named another way.</summary>
    public DateTime? CopiedAt { get; set; }

    /// <summary>The game's date in the save when it was copied; only the copies made before a write carry it.</summary>
    public DateOnly? GameDate { get; set; }
}
