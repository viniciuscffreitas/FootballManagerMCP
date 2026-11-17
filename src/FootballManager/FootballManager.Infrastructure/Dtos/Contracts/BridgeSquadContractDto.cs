using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;

namespace FootballManager.Infrastructure.Dtos.Contracts;

/// <summary>A first-team player's contract, keyed by the game's property paths.</summary>
internal sealed class BridgeSquadContractDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(ContractConstants.Wage)]
    public BridgeSortedValueDto? Wage { get; set; }

    [JsonPropertyName(ContractConstants.EndDate)]
    public string? EndDate { get; set; }

    [JsonPropertyName(ContractConstants.HasEndDate)]
    public bool? HasEndDate { get; set; }

    [JsonPropertyName(ContractConstants.TimeRemaining)]
    public string? TimeRemaining { get; set; }

    [JsonPropertyName(ContractConstants.Type)]
    public BridgeSortedValueDto? Type { get; set; }

    [JsonPropertyName(ContractConstants.AgreedPlayingTime)]
    public BridgeSortedValueDto? AgreedPlayingTime { get; set; }

    [JsonPropertyName(ContractConstants.ReleaseClause)]
    public BridgeSortedValueDto? ReleaseClause { get; set; }

    [JsonPropertyName(ContractConstants.ClausesCount)]
    public int? ClausesCount { get; set; }

    [JsonPropertyName(ContractConstants.HasRenewalOffer)]
    public bool? HasRenewalOffer { get; set; }

    [JsonPropertyName(ContractConstants.StaffAdvice)]
    public string? StaffAdvice { get; set; }
}
