using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>The club atmosphere, keyed by the game's property paths.</summary>
internal sealed class BridgeAtmosphereDto
{
    [JsonPropertyName(SquadDynamicsConstants.AtmosphereSummary)]
    public string? Summary { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.AtmosphereScore)]
    public int? Score { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.AtmosphereFactors)]
    public List<BridgeAtmosphereFactorDto>? Factors { get; set; }
}
