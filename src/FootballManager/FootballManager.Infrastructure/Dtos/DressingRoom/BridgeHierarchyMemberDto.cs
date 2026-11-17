using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>A player on one hierarchy level, keyed by the game's property paths.</summary>
internal sealed class BridgeHierarchyMemberDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }
}
