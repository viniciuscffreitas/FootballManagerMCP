using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Injuries;

/// <summary>The team's season injury summary, keyed by the game's property paths.</summary>
internal sealed class BridgeSeasonInjuriesDto
{
    [JsonPropertyName(InjuryConstants.SeasonCount)]
    public int? InjuriesThisSeason { get; set; }

    [JsonPropertyName(InjuryConstants.ComparedWithLastSeason)]
    public string? ComparedWithLastSeason { get; set; }

    [JsonPropertyName(InjuryConstants.ExpectedOverSeason)]
    public string? ExpectedOverSeason { get; set; }

    [JsonPropertyName(InjuryConstants.LeagueAverage)]
    public string? LeagueAverage { get; set; }

    [JsonPropertyName(InjuryConstants.SlightCount)]
    public int? SlightInjuries { get; set; }

    [JsonPropertyName(InjuryConstants.MinorCount)]
    public int? MinorInjuries { get; set; }

    [JsonPropertyName(InjuryConstants.ModerateCount)]
    public int? ModerateInjuries { get; set; }

    [JsonPropertyName(InjuryConstants.MajorCount)]
    public int? MajorInjuries { get; set; }

    [JsonPropertyName(InjuryConstants.HighRiskCount)]
    public int? HighRiskPlayers { get; set; }

    [JsonPropertyName(InjuryConstants.ExhaustedAndRecurring)]
    public string? ExhaustedAndRecurring { get; set; }

    [JsonPropertyName(InjuryConstants.ExpectedReturnsNextMatch)]
    public string? ExpectedReturnsNextMatch { get; set; }
}
