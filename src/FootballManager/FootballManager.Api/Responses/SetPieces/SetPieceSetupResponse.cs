namespace FootballManager.Api.Responses.SetPieces;

public class SetPieceSetupResponse
{
    public SetPiecePlayerResponse? Captain { get; set; }
    public SetPiecePlayerResponse? ViceCaptain { get; set; }
    public List<SetPiecePlayerResponse>? CaptainStandIns { get; set; }
    public List<SetPieceTakerListResponse> TakerLists { get; set; } = [];
}
