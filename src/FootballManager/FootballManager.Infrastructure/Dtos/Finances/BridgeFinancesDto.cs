using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>The user's club finance screen, keyed by the game's property paths.</summary>
internal sealed class BridgeFinancesDto
{
    [JsonPropertyName(FinancesConstants.Balance)]
    public long? Balance { get; set; }

    [JsonPropertyName(FinancesConstants.FinancialStatus)]
    public string? FinancialStatus { get; set; }

    [JsonPropertyName(FinancesConstants.TransferBudget)]
    public long? TransferBudget { get; set; }

    [JsonPropertyName(FinancesConstants.TransferBudgetNextSeason)]
    public long? TransferBudgetNextSeason { get; set; }

    [JsonPropertyName(FinancesConstants.TransferSpendingThisSeason)]
    public long? TransferSpendingThisSeason { get; set; }

    [JsonPropertyName(FinancesConstants.IncomeThisMonth)]
    public long? IncomeThisMonth { get; set; }

    [JsonPropertyName(FinancesConstants.ExpenditureThisMonth)]
    public long? ExpenditureThisMonth { get; set; }

    [JsonPropertyName(FinancesConstants.Wages)]
    public BridgeWagesDto? Wages { get; set; }

    [JsonPropertyName(FinancesConstants.IncomeLines)]
    public List<BridgeFinanceLineDto>? IncomeLines { get; set; }

    [JsonPropertyName(FinancesConstants.ExpenditureLines)]
    public List<BridgeFinanceLineDto>? ExpenditureLines { get; set; }

    [JsonPropertyName(FinancesConstants.NotableIncome)]
    public List<BridgeNotableIncomeDto>? NotableIncome { get; set; }

    [JsonPropertyName(FinancesConstants.BalanceHistory)]
    public List<BridgeGraphPointDto>? BalanceHistory { get; set; }

    [JsonPropertyName(FinancesConstants.BalanceProjection)]
    public BridgeProjectionDto? BalanceProjection { get; set; }
}
