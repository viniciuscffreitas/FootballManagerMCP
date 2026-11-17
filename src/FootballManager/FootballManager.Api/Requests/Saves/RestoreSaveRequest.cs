namespace FootballManager.Api.Requests.Saves;

public class RestoreSaveRequest
{
    /// <summary>The copy's file name in the backup folder, as the backup list shows it.</summary>
    public string Backup { get; set; } = "";
}
