namespace FootballManager.Api.Responses.SquadDynamics;

public class PlayerDynamicsResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? HierarchyLevel { get; set; }
    public List<string>? HappinessPositives { get; set; }
    public List<string>? HappinessNegatives { get; set; }
    public int Promises { get; set; }
}
