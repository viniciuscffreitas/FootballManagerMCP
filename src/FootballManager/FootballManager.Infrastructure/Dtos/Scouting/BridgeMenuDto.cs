using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A menu action's entries, read from game.Action for the records it applies to.</summary>
internal sealed class BridgeMenuDto
{
    [JsonPropertyName(ScoutingConstants.MenuItems)]
    /// <summary>Null when the game left the entries out: unknown, not an empty menu.</summary>
    public List<BridgeMenuItemDto>? Items { get; set; }
}
