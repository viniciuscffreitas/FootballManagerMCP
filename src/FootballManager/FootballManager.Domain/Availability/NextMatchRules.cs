namespace FootballManager.Domain.Availability;

/// <summary>The squad rules of the team's next match, as the game's team selection screen checks them.</summary>
public class NextMatchRules
{
    /// <summary>The next match's competition; null when the team has no match left.</summary>
    public string? Competition { get; set; }

    /// <summary>The competition's registration and eligibility rules for the match; empty when it has none.</summary>
    public List<string> EligibilityRules { get; set; } = [];

    /// <summary>The competition's squad size rule, e.g. "Maximum squad size of 50 players".</summary>
    public string? MaxSquadRule { get; set; }

    /// <summary>How the squad stands against that rule, e.g. "50/50".</summary>
    public string? MaxSquadRuleStatus { get; set; }
}
