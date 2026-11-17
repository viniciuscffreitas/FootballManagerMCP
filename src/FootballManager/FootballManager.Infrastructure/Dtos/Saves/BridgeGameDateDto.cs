using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Saves;

/// <summary>The game's current date, on the game root.</summary>
internal sealed class BridgeGameDateDto
{
    // ISO, e.g. "2026-03-10T00:00:00".
    [JsonPropertyName(SaveConstants.CurrentGameDate)]
    public string? CurrentGameDate { get; set; }
}
