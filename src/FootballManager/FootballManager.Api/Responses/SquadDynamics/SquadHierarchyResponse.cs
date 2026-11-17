namespace FootballManager.Api.Responses.SquadDynamics;

public class SquadHierarchyResponse
{
    public List<HierarchyMemberResponse> TeamLeaders { get; set; } = [];
    public List<HierarchyMemberResponse> HighlyInfluential { get; set; } = [];
    public List<HierarchyMemberResponse> Influential { get; set; } = [];
    public List<HierarchyMemberResponse> Other { get; set; } = [];
}
