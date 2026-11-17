using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Availability;

/// <summary>One of the team's active bans, keyed by the game's property paths.</summary>
internal sealed class BridgeBanDto
{
    [JsonPropertyName(AvailabilityConstants.BanPlayerId)]
    public int? PlayerId { get; set; }

    [JsonPropertyName(AvailabilityConstants.BanPlayerName)]
    public string? PlayerName { get; set; }

    [JsonPropertyName(AvailabilityConstants.BanScope)]
    public string? Scope { get; set; }

    [JsonPropertyName(AvailabilityConstants.BanMatchesRemaining)]
    public int? MatchesRemaining { get; set; }

    [JsonPropertyName(AvailabilityConstants.BanDaysRemaining)]
    public int? DaysRemaining { get; set; }

    [JsonPropertyName(AvailabilityConstants.BanReason)]
    public string? Reason { get; set; }
}
