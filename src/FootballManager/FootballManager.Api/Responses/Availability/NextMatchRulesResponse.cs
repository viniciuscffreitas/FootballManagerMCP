namespace FootballManager.Api.Responses.Availability;

public class NextMatchRulesResponse
{
    public string? Competition { get; set; }

    /// <summary>The competition's registration and eligibility rules; empty when it has none.</summary>
    public List<string> EligibilityRules { get; set; } = [];

    public string? MaxSquadRule { get; set; }

    /// <summary>The squad against the size rule, e.g. "50/50".</summary>
    public string? MaxSquadRuleStatus { get; set; }
}
