namespace FootballManager.Api.Responses.SquadDynamics;

public class SquadDynamicsResponse
{
    public required SquadHierarchyResponse Hierarchy { get; set; }
    public required AtmosphereResponse Atmosphere { get; set; }
    public List<HappinessIssueResponse> Issues { get; set; } = [];

    /// <summary>The promises the manager has made to players.</summary>
    public List<PlayerPromiseResponse> Promises { get; set; } = [];

    public List<PlayerDynamicsResponse> Players { get; set; } = [];
}
