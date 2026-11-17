namespace FootballManager.Domain.Youth;

/// <summary>The club's youth set-up as the game rates it, and its next youth intake.</summary>
public class YouthSetup
{
    /// <summary>The highest youth team, e.g. "Santos Sub-20".</summary>
    public string? TeamName { get; set; }

    /// <summary>The day of the next youth intake (yyyy-MM-dd).</summary>
    public string? IntakeDate { get; set; }

    public int? DaysUntilIntake { get; set; }

    /// <summary>The youth facilities' rating, out of 20.</summary>
    public int? FacilitiesRating { get; set; }

    /// <summary>The youth facilities as the game words them, short and long, e.g. "Good" and "Good youth facilities".</summary>
    public string? Facilities { get; set; }

    public string? FacilitiesDescription { get; set; }

    /// <summary>The youth coaching level, out of 20.</summary>
    public int? CoachingLevel { get; set; }

    /// <summary>The youth recruitment level as the game holds it, a small number (3 for Santos).</summary>
    public int? Recruitment { get; set; }

    /// <summary>Candidates of the current intake still to be decided on; 0 outside the intake.</summary>
    public int? Candidates { get; set; }
}
