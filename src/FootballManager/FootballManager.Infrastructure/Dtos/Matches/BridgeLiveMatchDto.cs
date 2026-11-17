using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>
/// The match being played as its screen reads it. The live-only values have no type the game declares, so they're
/// kept as sent and read as numbers where they are.
/// </summary>
internal sealed class BridgeLiveMatchDto
{
    [JsonPropertyName(MatchConstants.Competition)]
    public string? Competition { get; set; }

    [JsonPropertyName(MatchConstants.HomeTeamId)]
    public int? HomeTeamId { get; set; }

    [JsonPropertyName(MatchConstants.HomeTeamName)]
    public string? HomeTeamName { get; set; }

    [JsonPropertyName(MatchConstants.AwayTeamId)]
    public int? AwayTeamId { get; set; }

    [JsonPropertyName(MatchConstants.AwayTeamName)]
    public string? AwayTeamName { get; set; }

    [JsonPropertyName(LiveMatchConstants.Minute)]
    public JsonElement? Minute { get; set; }

    [JsonPropertyName(LiveMatchConstants.Clock)]
    public string? Clock { get; set; }

    [JsonPropertyName(LiveMatchConstants.HomeScore)]
    public JsonElement? LiveHomeScore { get; set; }

    [JsonPropertyName(LiveMatchConstants.AwayScore)]
    public JsonElement? LiveAwayScore { get; set; }

    /// <summary>The fixture's score: text, -1 until the game writes it.</summary>
    [JsonPropertyName(MatchConstants.HomeScore), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? HomeScore { get; set; }

    [JsonPropertyName(MatchConstants.AwayScore), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? AwayScore { get; set; }

    [JsonPropertyName(MatchConstants.Incidents)]
    public List<BridgeIncidentDto>? Incidents { get; set; }
}
