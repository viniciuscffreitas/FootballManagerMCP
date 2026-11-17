namespace FootballManager.Api.Responses.Saves;

public class SaveRestoreResponse
{
    public bool Applied { get; set; }

    /// <summary>The real save the copy replaces.</summary>
    public string Save { get; set; } = "";

    /// <summary>The copy put back, with its club and game date.</summary>
    public SaveBackupResponse Restored { get; set; } = new();

    /// <summary>The file in the backup folder that keeps the save as it was before the restore.</summary>
    public string KeptAs { get; set; } = "";
}
