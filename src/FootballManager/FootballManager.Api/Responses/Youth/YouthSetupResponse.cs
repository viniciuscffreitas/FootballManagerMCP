namespace FootballManager.Api.Responses.Youth;

public class YouthSetupResponse
{
    public string? TeamName { get; set; }
    public string? IntakeDate { get; set; }
    public int? DaysUntilIntake { get; set; }

    /// <summary>Out of 20.</summary>
    public int? FacilitiesRating { get; set; }

    public string? Facilities { get; set; }
    public string? FacilitiesDescription { get; set; }

    /// <summary>Out of 20.</summary>
    public int? CoachingLevel { get; set; }

    /// <summary>The game's youth recruitment level, a small number.</summary>
    public int? Recruitment { get; set; }

    public int? Candidates { get; set; }
}
