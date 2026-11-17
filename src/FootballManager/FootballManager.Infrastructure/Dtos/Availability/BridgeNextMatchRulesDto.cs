using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Availability;

/// <summary>The team's next match rules, keyed by the game's property paths.</summary>
internal sealed class BridgeNextMatchRulesDto
{
    [JsonPropertyName(AvailabilityConstants.NextCompetition)]
    public string? Competition { get; set; }

    // A list; empty for a competition without registration rules (seen in the Brasileirão). Items are kept as the
    // game sends them, since no save has shown one yet.
    [JsonPropertyName(AvailabilityConstants.EligibilityRule)]
    public List<JsonElement>? EligibilityRules { get; set; }

    [JsonPropertyName(AvailabilityConstants.MaxSquadRule)]
    public string? MaxSquadRule { get; set; }

    [JsonPropertyName(AvailabilityConstants.MaxSquadRuleStatus)]
    public string? MaxSquadRuleStatus { get; set; }
}
