using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Youth;

/// <summary>The club's youth set-up and loans summary, keyed by the game's property paths.</summary>
internal sealed class BridgeYouthSetupDto
{
    [JsonPropertyName(YouthConstants.LoanReport)]
    public string? LoanReport { get; set; }

    [JsonPropertyName(YouthConstants.PlayersSuggestedForLoan)]
    public int? PlayersSuggestedForLoan { get; set; }

    [JsonPropertyName(YouthConstants.YouthTeamName)]
    public string? TeamName { get; set; }

    [JsonPropertyName(YouthConstants.IntakeDate)]
    public string? IntakeDate { get; set; }

    [JsonPropertyName(YouthConstants.DaysUntilIntake)]
    public int? DaysUntilIntake { get; set; }

    [JsonPropertyName(YouthConstants.FacilityRating)]
    public BridgeFacilityRatingDto? Facilities { get; set; }

    [JsonPropertyName(YouthConstants.CoachingLevel)]
    public int? CoachingLevel { get; set; }

    [JsonPropertyName(YouthConstants.Recruitment)]
    public int? Recruitment { get; set; }

    [JsonPropertyName(YouthConstants.Candidates)]
    public int? Candidates { get; set; }
}
