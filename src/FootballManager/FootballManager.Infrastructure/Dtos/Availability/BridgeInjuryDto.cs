using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Availability;

/// <summary>A first-team player's current injury, keyed by the game's property paths.</summary>
internal sealed class BridgeInjuryDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(AvailabilityConstants.InjuryAvailability)]
    public int? Availability { get; set; }

    [JsonPropertyName(AvailabilityConstants.InjuryDescription)]
    public string? Description { get; set; }

    [JsonPropertyName(AvailabilityConstants.InjuryExpectedReturn)]
    public string? ExpectedReturn { get; set; }

    [JsonPropertyName(AvailabilityConstants.InjuryReturnDays)]
    public int? ReturnDays { get; set; }
}
