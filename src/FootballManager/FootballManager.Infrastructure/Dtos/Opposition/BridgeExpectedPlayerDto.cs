using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>
/// A place of the expected lineup: a tactics position mask (see PositionMasks), an in-possession role mask with the
/// duty's bit added, and the player as a reference to his record.
/// </summary>
internal sealed class BridgeExpectedPlayerDto
{
    public ulong? Position { get; set; }
    public ulong? Role { get; set; }

    [JsonPropertyName("player")]
    public BridgeReferenceDto? Player { get; set; }
}
