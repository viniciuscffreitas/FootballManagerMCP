using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Staff;

/// <summary>A first-team staff member, keyed by the game's property paths.</summary>
internal sealed class BridgeStaffMemberDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(GameConstants.Age)]
    public int? Age { get; set; }

    [JsonPropertyName(StaffConstants.Job)]
    public string? Job { get; set; }

    [JsonPropertyName(StaffConstants.CoachingStyle)]
    public string? CoachingStyle { get; set; }

    /// <summary>The weekly wage as the game writes it, e.g. "R$13 mil p/s".</summary>
    [JsonPropertyName(StaffConstants.Wage)]
    public string? Wage { get; set; }

    /// <summary>yyyy-MM-dd; the game dates a contract without an end 1900.</summary>
    [JsonPropertyName(StaffConstants.ContractEnd)]
    public string? ContractEnd { get; set; }

    [JsonPropertyName(StaffConstants.CoachingAttributes)]
    public List<BridgeStaffAttributeDto>? CoachingAttributes { get; set; }

    [JsonPropertyName(StaffConstants.MedicalAttributes)]
    public List<BridgeStaffAttributeDto>? MedicalAttributes { get; set; }
}
