using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Injuries;

/// <summary>One record of a player's InjuriesHistory, keyed by the game's property names.</summary>
internal sealed class BridgePastInjuryDto
{
    [JsonPropertyName(InjuryConstants.Date)]
    public string? Date { get; set; }

    [JsonPropertyName(InjuryConstants.Description)]
    public string? Description { get; set; }

    [JsonPropertyName(InjuryConstants.Duration)]
    public string? Duration { get; set; }

    [JsonPropertyName(InjuryConstants.DurationDays)]
    public int? DurationDays { get; set; }

    // 0 slight, 1 minor, 2 moderate, 3 major: a torn ACL is 3, a bruise 0.
    [JsonPropertyName(InjuryConstants.Severity)]
    public int? Severity { get; set; }

    [JsonPropertyName(InjuryConstants.Cause)]
    public string? Cause { get; set; }

    [JsonPropertyName(InjuryConstants.ReceivedIn)]
    public string? ReceivedIn { get; set; }

    [JsonPropertyName(InjuryConstants.IsActive)]
    public bool? IsActive { get; set; }
}
