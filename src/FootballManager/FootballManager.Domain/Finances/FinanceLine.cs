namespace FootballManager.Domain.Finances;

/// <summary>One line of this month's income or expenditure, e.g. "Receita de TV" "R$8.01 MI".</summary>
public class FinanceLine
{
    public required string Name { get; set; }
    public string? Amount { get; set; }

    /// <summary>The line's share of the month's total, in percent.</summary>
    public int? Percentage { get; set; }

    /// <summary>The change against last month, in percent.</summary>
    public int? ChangeFromLastMonth { get; set; }
}
