namespace FootballManager.Domain.Finances;

/// <summary>
/// The user's club finances in the currency the game's screens show. Texts the game writes itself pass as they are;
/// the amounts it only sends in its own unit are converted at the rate its wage table shows, to the nearest unit.
/// </summary>
public class ClubFinances
{
    public string? Balance { get; set; }
    public string? FinancialStatus { get; set; }
    public string? TransferBudget { get; set; }
    public string? TransferBudgetNextSeason { get; set; }
    public string? TransferSpendingThisSeason { get; set; }
    public string? IncomeThisMonth { get; set; }
    public string? ExpenditureThisMonth { get; set; }

    public WageSummary Wages { get; set; } = new();

    /// <summary>The wage bill by kind of player, only kinds the squad has.</summary>
    public List<WageGroup> WageGroups { get; set; } = [];

    public List<FinanceLine> IncomeLines { get; set; } = [];
    public List<FinanceLine> ExpenditureLines { get; set; } = [];
    public List<NotableIncome> NotableIncome { get; set; } = [];
    public List<BalanceEntry> BalanceHistory { get; set; } = [];
    public List<BalanceEntry> BalanceProjection { get; set; } = [];

    /// <summary>
    /// Units of the screens' currency per unit of the game's (pounds sterling), read from the wage table; null when
    /// the table had no amount to read it from, and the converted amounts are then in pounds.
    /// </summary>
    public decimal? ConversionRate { get; set; }
}
