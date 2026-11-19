using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Registration;

internal sealed class BridgeRegistrationPlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    /// <summary>-1 when he has none.</summary>
    [JsonPropertyName(RegistrationConstants.SquadNumber)]
    public int? SquadNumber { get; set; }

    [JsonPropertyName(RegistrationConstants.IsGoalkeeper)]
    public bool? IsGoalkeeper { get; set; }
}
