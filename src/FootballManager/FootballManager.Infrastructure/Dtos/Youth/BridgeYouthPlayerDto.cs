using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Youth;

/// <summary>A player of the highest youth team, keyed by the game's property paths.</summary>
internal sealed class BridgeYouthPlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(GameConstants.Age)]
    public int? Age { get; set; }

    [JsonPropertyName(GameConstants.Positions)]
    public string? Positions { get; set; }

    [JsonPropertyName(GameConstants.Morale)]
    public string? Morale { get; set; }

    [JsonPropertyName(YouthConstants.ContractEndDate)]
    public string? ContractEndDate { get; set; }
}
