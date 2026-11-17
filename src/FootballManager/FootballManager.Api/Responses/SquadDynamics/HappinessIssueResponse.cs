namespace FootballManager.Api.Responses.SquadDynamics;

public class HappinessIssueResponse
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Severity { get; set; }
    public string? Description { get; set; }
    public int PlayersAgreeing { get; set; }
    public bool HasPromises { get; set; }
}
