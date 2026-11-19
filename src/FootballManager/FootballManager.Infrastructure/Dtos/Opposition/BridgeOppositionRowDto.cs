using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>
/// One row: the player by reference to his record, his tactics position mask, his place in the expected lineup and the
/// four instructions as the game's option numbers (0 is none).
/// </summary>
internal sealed class BridgeOppositionRowDto
{
    [JsonPropertyName("player")]
    public BridgeReferenceDto? Player { get; set; }

    public ulong? Position { get; set; }
    public int? Index { get; set; }
    public int? OppositionPlayerMarking { get; set; }
    public int? OppositionPlayerPressingTrigger { get; set; }
    public int? OppositionPlayerTackling { get; set; }
    public int? OppositionPlayerShowOnToFoot { get; set; }
}
