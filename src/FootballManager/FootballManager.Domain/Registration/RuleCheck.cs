namespace FootballManager.Domain.Registration;

/// <summary>A registration rule against a planned change: its value now and after the change, when the plan can tell.</summary>
public class RuleCheck
{
    public required string Rule { get; set; }
    public int? Before { get; set; }

    /// <summary>The value after the change; null when the plan can't work it out (the game checks it on confirming).</summary>
    public int? After { get; set; }

    public int? Limit { get; set; }

    /// <summary>Whether the change breaks the rule; null when the plan can't tell.</summary>
    public bool? Broken { get; set; }
}
