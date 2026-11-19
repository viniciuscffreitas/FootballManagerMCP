namespace FootballManager.Infrastructure.Dtos.Registration;

/// <summary>A registration rule as the game lists it, e.g. RuleType 9, "47/50 Máx.".</summary>
internal sealed class BridgeRegistrationRuleDto
{
    public int? RuleType { get; set; }
    public string? SquadSelectionRuleTitle { get; set; }
    public string? SquadSelectionRuleString { get; set; }
    public int? PlayerValue { get; set; }
    public string? SquadSelectionRuleValue { get; set; }

    /// <summary>1 when the rule is met.</summary>
    public int? CurrentlyMeetsRule { get; set; }
}
