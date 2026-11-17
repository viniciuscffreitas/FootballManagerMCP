namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>The wages screen's figures, as texts in the screens' currency, and the wage bill by kind of player.</summary>
internal sealed class BridgeWagesDto
{
    public string? CommittedWageTotalValueWithShortText { get; set; }
    public string? CurrentWageBudgetValueWithText { get; set; }
    public string? RemainingBudgetValueWithText { get; set; }
    public string? NextSeasonWageBudgetValueWithText { get; set; }
    public double? WageBudgetPercentage { get; set; }
    public List<BridgeWageGroupDto>? PlayerTypeWageData { get; set; }
}
