using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>A reason the game gives for the atmosphere, as an item of its list.</summary>
internal sealed class BridgeAtmosphereFactorDto
{
    [JsonPropertyName(SquadDynamicsConstants.FactorText)]
    public string? Text { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.FactorValue)]
    public int? Value { get; set; }
}
