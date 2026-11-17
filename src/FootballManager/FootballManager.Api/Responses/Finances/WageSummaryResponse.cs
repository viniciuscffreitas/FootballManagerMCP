namespace FootballManager.Api.Responses.Finances;

public class WageSummaryResponse
{
    public string? Committed { get; set; }
    public string? Budget { get; set; }
    public string? Remaining { get; set; }
    public string? NextSeasonBudget { get; set; }
    public double? BudgetUsedPercentage { get; set; }
}
