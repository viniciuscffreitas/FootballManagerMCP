using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>A first-team player's hierarchy level and happiness, keyed by the game's property paths.</summary>
internal sealed class BridgePlayerDynamicsDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.HierarchyLevel)]
    public string? HierarchyLevel { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.HappinessPositives)]
    public List<BridgeHappinessReasonDto>? HappinessPositives { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.HappinessNegatives)]
    public List<BridgeHappinessReasonDto>? HappinessNegatives { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.PromisesCount)]
    public int? Promises { get; set; }
}
