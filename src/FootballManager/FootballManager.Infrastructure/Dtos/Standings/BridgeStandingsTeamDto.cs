using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Standings;

/// <summary>The user's team as the league table needs it, keyed by the game's property paths.</summary>
internal sealed class BridgeStandingsTeamDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(TeamConstants.DivisionName)]
    public string? League { get; set; }

    [JsonPropertyName(StandingsConstants.LeagueStage)]
    public BridgeReferenceDto? Stage { get; set; }

    [JsonPropertyName(StandingsConstants.MediaExpectedPosition)]
    public int? MediaExpectedPosition { get; set; }
}
