namespace FootballManager.Domain.Saves;

/// <summary>The copies in the backup folder, newest first.</summary>
public class SaveBackupList
{
    public required string Folder { get; set; }
    public List<SaveBackup> Backups { get; set; } = [];
}
