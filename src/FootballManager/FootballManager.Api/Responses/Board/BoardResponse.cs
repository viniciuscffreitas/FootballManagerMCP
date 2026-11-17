namespace FootballManager.Api.Responses.Board;

public class BoardResponse
{
    public required BoardConfidenceResponse Board { get; set; }
    public required SupporterConfidenceResponse Supporters { get; set; }
    public VisionObjectiveResponse? ThisYearObjective { get; set; }
    public VisionObjectiveResponse? FinalObjective { get; set; }
    public List<string> SupporterCulture { get; set; } = [];
    public List<string> BoardCulture { get; set; } = [];
    public int BoardRequests { get; set; }
    public int ActiveBoardRequests { get; set; }

    /// <summary>Promises the user made to the board; empty when there are none.</summary>
    public List<string> Promises { get; set; } = [];
}
