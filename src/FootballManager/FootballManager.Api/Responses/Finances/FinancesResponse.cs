namespace FootballManager.Api.Responses.Finances;

public class FinancesResponse
{
    /// <summary>The club's balance in the screens' currency, converted at <see cref="ConversionRate"/>.</summary>
    public string? Balance { get; set; }

    public string? FinancialStatus { get; set; }
    public string? TransferBudget { get; set; }
    public string? TransferBudgetNextSeason { get; set; }
    public string? TransferSpendingThisSeason { get; set; }
    public string? IncomeThisMonth { get; set; }
    public string? ExpenditureThisMonth { get; set; }
    public required WageSummaryResponse Wages { get; set; }
    public List<WageGroupResponse> WageGroups { get; set; } = [];
    public List<FinanceLineResponse> IncomeLines { get; set; } = [];
    public List<FinanceLineResponse> ExpenditureLines { get; set; } = [];
    public List<NotableIncomeResponse> NotableIncome { get; set; } = [];
    public List<BalanceEntryResponse> BalanceHistory { get; set; } = [];
    public List<BalanceEntryResponse> BalanceProjection { get; set; } = [];

    /// <summary>Units of the screens' currency per pound; null when amounts had to stay in pounds.</summary>
    public decimal? ConversionRate { get; set; }
}
