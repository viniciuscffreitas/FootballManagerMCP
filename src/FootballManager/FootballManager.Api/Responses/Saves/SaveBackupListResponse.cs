namespace FootballManager.Api.Responses.Saves;

public class SaveBackupListResponse
{
    public string Folder { get; set; } = "";
    public List<SaveBackupResponse> Backups { get; set; } = [];
}
