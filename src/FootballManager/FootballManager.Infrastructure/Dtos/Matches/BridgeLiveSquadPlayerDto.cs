using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A first-team player, to name the players a match refers to.</summary>
internal sealed class BridgeLiveSquadPlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }
}
