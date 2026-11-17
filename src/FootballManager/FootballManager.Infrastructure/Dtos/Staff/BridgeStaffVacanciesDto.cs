using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Staff;

/// <summary>The club's open staff jobs.</summary>
internal sealed class BridgeStaffVacanciesDto
{
    [JsonPropertyName(StaffConstants.Vacancies)]
    public int? Vacancies { get; set; }
}
