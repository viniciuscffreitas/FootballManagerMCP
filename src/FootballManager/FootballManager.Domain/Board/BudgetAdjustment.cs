namespace FootballManager.Domain.Board;

/// <summary>A move of money between the transfer and the wage budget, as planned or sent.</summary>
public class BudgetAdjustment
{
    public bool Applied { get; set; }

    /// <summary>The two budgets as the club's menu sums them up ("R$30 MI / R$52.5 mil p/s").</summary>
    public string? Budgets { get; set; }

    /// <summary>Steps of the game's own increment asked for, positive from transfers to wages; null only opens the panel and reads it.</summary>
    public int? Steps { get; set; }

    /// <summary>After an apply: the panel before the change.</summary>
    public BudgetPanel? Before { get; set; }

    /// <summary>After an apply that moved the slider: the panel once the game took the change.</summary>
    public BudgetPanel? After { get; set; }

    public List<string> Notes { get; set; } = [];
}
