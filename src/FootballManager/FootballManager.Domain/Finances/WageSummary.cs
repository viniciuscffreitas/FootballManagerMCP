namespace FootballManager.Domain.Finances;

/// <summary>The club's weekly wage bill against its budget, as the game's wages screen writes it.</summary>
public class WageSummary
{
    /// <summary>What the club has committed to pay each week, e.g. "R$3.396.473 p/s".</summary>
    public string? Committed { get; set; }

    public string? Budget { get; set; }
    public string? Remaining { get; set; }

    /// <summary>Next season's budget, or the game's word for one not set yet ("Ainda não anunciado").</summary>
    public string? NextSeasonBudget { get; set; }

    /// <summary>The share of the wage budget in use, in percent.</summary>
    public double? BudgetUsedPercentage { get; set; }
}
