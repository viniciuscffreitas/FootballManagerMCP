using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Injuries;

/// <summary>A first-team player's medical record, keyed by the game's property paths.</summary>
internal sealed class BridgeMedicalRecordDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(InjuryConstants.History)]
    public List<BridgePastInjuryDto>? History { get; set; }

    [JsonPropertyName(InjuryConstants.ThisSeason)]
    public int? InjuriesThisSeason { get; set; }

    [JsonPropertyName(InjuryConstants.RiskLevel)]
    public int? RiskLevel { get; set; }

    [JsonPropertyName(InjuryConstants.RiskOverview)]
    public string? RiskOverview { get; set; }

    [JsonPropertyName(InjuryConstants.RiskReason)]
    public string? RiskReason { get; set; }
}
