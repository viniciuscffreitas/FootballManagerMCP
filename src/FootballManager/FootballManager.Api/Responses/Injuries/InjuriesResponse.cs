namespace FootballManager.Api.Responses.Injuries;

public class InjuriesResponse
{
    public List<PlayerMedicalRecordResponse> Players { get; set; } = [];
    public required SeasonInjurySummaryResponse Season { get; set; }
}
