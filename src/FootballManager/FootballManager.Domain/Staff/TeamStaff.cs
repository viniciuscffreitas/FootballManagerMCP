namespace FootballManager.Domain.Staff;

/// <summary>The first team's staff, the club's staff counts by department and the jobs it has open.</summary>
public class TeamStaff
{
    public required string TeamName { get; set; }

    /// <summary>The staff the game lists for the first team, the user's manager among them.</summary>
    public List<StaffMember> Members { get; set; } = [];

    /// <summary>The club's whole staff, every team and department.</summary>
    public int? AllCount { get; set; }

    public int? CoachingCount { get; set; }
    public int? MedicalCount { get; set; }

    /// <summary>The analyst the game names for the team (AnalystPerson); null when it names none.</summary>
    public string? Analyst { get; set; }

    /// <summary>Staff jobs the club has open.</summary>
    public int? Vacancies { get; set; }
}
