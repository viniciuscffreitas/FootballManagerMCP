using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>A promise the user's manager made to a player, keyed by the game's property paths.</summary>
internal sealed class BridgePromiseDto
{
    [JsonPropertyName(SquadDynamicsConstants.PromiseTitle)]
    public string? Title { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.PromiseDescription)]
    public string? Description { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.PromiseStatus)]
    public string? Status { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.PromiseDaysRemaining)]
    public int? DaysRemaining { get; set; }

    // Kept as the game sends it; no save has shown a promise yet.
    [JsonPropertyName(SquadDynamicsConstants.PromiseEndDate)]
    public JsonElement? EndDate { get; set; }
}
