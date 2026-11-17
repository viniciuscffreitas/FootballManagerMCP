using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>The user's next opponent and the threat the game sees in it, read on the user's team.</summary>
internal sealed class BridgeNextOpponentDto
{
    [JsonPropertyName(OppositionConstants.NextOpponentId)]
    public int? Id { get; set; }

    [JsonPropertyName(OppositionConstants.NextOpponentName)]
    public string? Name { get; set; }

    [JsonPropertyName(OppositionConstants.ThreatLevel)]
    public string? ThreatLevel { get; set; }
}
