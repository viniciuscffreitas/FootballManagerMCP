namespace FootballManager.Domain.Finances;

/// <summary>
/// What the game's finance screen says about the user's club. Texts are the game's own, already in the currency its
/// screens show (e.g. "R$3.396.473"); the amounts it sends only as numbers are in its own unit, pounds sterling.
/// </summary>
public class FinanceReadout
{
    /// <summary>The club's balance, in the game's unit.</summary>
    public long? Balance { get; set; }

    /// <summary>How the board rates the club's finances, as the game words it, e.g. "Instável".</summary>
    public string? FinancialStatus { get; set; }

    public long? TransferBudget { get; set; }
    public long? TransferBudgetNextSeason { get; set; }
    public long? TransferSpendingThisSeason { get; set; }
    public long? IncomeThisMonth { get; set; }
    public long? ExpenditureThisMonth { get; set; }

    public WageSummary Wages { get; set; } = new();

    /// <summary>The wage bill by kind of player, each with its total as text and in the game's unit.</summary>
    public List<WageGroup> WageGroups { get; set; } = [];

    public List<FinanceLine> IncomeLines { get; set; } = [];
    public List<FinanceLine> ExpenditureLines { get; set; } = [];
    public List<NotableIncome> NotableIncome { get; set; } = [];

    /// <summary>The balance at the start of each past month, in the game's unit.</summary>
    public List<BalanceAt> BalanceHistory { get; set; } = [];

    /// <summary>The balance the game projects at the end of each coming season, in the game's unit.</summary>
    public List<BalanceAt> BalanceProjection { get; set; } = [];
}
