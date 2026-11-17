using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>A reason a player is happy or unhappy, as an item of his list; the game's Image is its icon.</summary>
internal sealed class BridgeHappinessReasonDto
{
    [JsonPropertyName(SquadDynamicsConstants.ReasonText)]
    public string? Text { get; set; }
}
