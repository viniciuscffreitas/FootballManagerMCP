namespace FootballManager.Domain.Saves;

/// <summary>A copy put back over the real save, or the plan for it on a dry run.</summary>
public class SaveRestore
{
    public bool Applied { get; set; }

    /// <summary>The real save the copy replaces, its file name without ".fm".</summary>
    public required string Save { get; set; }

    /// <summary>The copy put back, with its club and game date.</summary>
    public required SaveBackup Restored { get; set; }

    /// <summary>Where the real save as it was before the restore is kept, in the backup folder.</summary>
    public required string KeptAs { get; set; }
}
