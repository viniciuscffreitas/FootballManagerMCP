using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Standings;

/// <summary>One row of the league table, keyed by the game's property paths.</summary>
internal sealed class BridgeTableRowDto
{
    [JsonPropertyName(GameConstants.TeamId)]
    public int? TeamId { get; set; }

    [JsonPropertyName(StandingsConstants.TeamName)]
    public string? TeamName { get; set; }

    [JsonPropertyName(StandingsConstants.Position)]
    public int? Position { get; set; }

    [JsonPropertyName(StandingsConstants.Played)]
    public int? Played { get; set; }

    [JsonPropertyName(StandingsConstants.Won)]
    public int? Won { get; set; }

    [JsonPropertyName(StandingsConstants.Drawn)]
    public int? Drawn { get; set; }

    [JsonPropertyName(StandingsConstants.Lost)]
    public int? Lost { get; set; }

    [JsonPropertyName(StandingsConstants.GoalsFor)]
    public int? GoalsFor { get; set; }

    [JsonPropertyName(StandingsConstants.GoalsAgainst)]
    public int? GoalsAgainst { get; set; }

    [JsonPropertyName(StandingsConstants.GoalDifference)]
    public int? GoalDifference { get; set; }

    [JsonPropertyName(StandingsConstants.Points)]
    public int? Points { get; set; }

    [JsonPropertyName(StandingsConstants.Fate)]
    public string? Fate { get; set; }

    [JsonPropertyName(StandingsConstants.Rising)]
    public bool? Rising { get; set; }

    [JsonPropertyName(StandingsConstants.Falling)]
    public bool? Falling { get; set; }
}
