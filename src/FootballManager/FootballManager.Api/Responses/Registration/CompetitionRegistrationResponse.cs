namespace FootballManager.Api.Responses.Registration;

public class CompetitionRegistrationResponse
{
    public required string Name { get; set; }
    public string? Competition { get; set; }
    public int? CompetitionId { get; set; }
    public bool WindowOpen { get; set; }
    public string? Window { get; set; }
    public string? Deadline { get; set; }
    public int? MaxSquadSize { get; set; }
    public bool MeetsRules { get; set; }
    public string? RulesSummary { get; set; }
    public string? Notes { get; set; }
    public List<RegistrationRuleResponse> Rules { get; set; } = [];
    public List<RegistrationPlayerResponse> Registered { get; set; } = [];
    public List<RegistrationPlayerResponse> Unregistered { get; set; } = [];
}
