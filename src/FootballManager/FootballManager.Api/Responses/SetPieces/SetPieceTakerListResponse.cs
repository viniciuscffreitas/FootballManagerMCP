namespace FootballManager.Api.Responses.SetPieces;

public class SetPieceTakerListResponse
{
    public required string Key { get; set; }
    public string? Label { get; set; }
    public List<SetPiecePlayerResponse> Takers { get; set; } = [];
}
