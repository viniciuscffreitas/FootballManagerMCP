namespace FootballManager.Api.Responses.Staff;

public class StaffResponse
{
    public required string TeamName { get; set; }

    /// <summary>The club's whole staff, every team and department.</summary>
    public int? AllCount { get; set; }

    public int? CoachingCount { get; set; }
    public int? MedicalCount { get; set; }
    /// <summary>The analyst the game names for the team, not necessarily the performance analyst.</summary>
    public string? Analyst { get; set; }

    /// <summary>Staff jobs the club has open.</summary>
    public int? Vacancies { get; set; }

    /// <summary>The first team's staff, the user's manager among them.</summary>
    public List<StaffMemberResponse> Members { get; set; } = [];
}
