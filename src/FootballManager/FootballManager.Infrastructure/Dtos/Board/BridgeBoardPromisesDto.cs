using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Board;

/// <summary>The promises the user's manager made to the board, keyed by the game's property path.</summary>
internal sealed class BridgeBoardPromisesDto
{
    // A list; empty in the save read so far, so items are kept as the game sends them.
    [JsonPropertyName(BoardConstants.Promises)]
    public List<JsonElement>? Promises { get; set; }
}
