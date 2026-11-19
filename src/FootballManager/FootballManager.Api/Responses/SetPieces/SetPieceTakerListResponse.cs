namespace FootballManager.Api.Responses.SetPieces;

public class SetPieceTakerListResponse
{
    public required string Key { get; set; }
    public string? Label { get; set; }

    /// <summary>False for a left or right list until its side is checked against the screen; null without a side.</summary>
    public bool? SideVerified { get; set; }
    public List<SetPiecePlayerResponse> Takers { get; set; } = [];
}
