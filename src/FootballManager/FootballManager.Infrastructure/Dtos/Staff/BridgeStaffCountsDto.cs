using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Staff;

/// <summary>The first team's name and staff counts by department, keyed by the game's property paths.</summary>
internal sealed class BridgeStaffCountsDto
{
    [JsonPropertyName(GameConstants.Name)]
    public string? TeamName { get; set; }

    [JsonPropertyName(StaffConstants.AllCount)]
    public int? AllCount { get; set; }

    [JsonPropertyName(StaffConstants.CoachingCount)]
    public int? CoachingCount { get; set; }

    [JsonPropertyName(StaffConstants.MedicalCount)]
    public int? MedicalCount { get; set; }

    [JsonPropertyName(StaffConstants.Analyst)]
    public string? Analyst { get; set; }
}
