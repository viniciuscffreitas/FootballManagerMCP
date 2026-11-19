namespace FootballManager.Api.Responses.Board;

public class BoardMenuEntryResponse
{
    public required string Label { get; set; }
    public bool Disabled { get; set; }
    public string? Confirmation { get; set; }
}
