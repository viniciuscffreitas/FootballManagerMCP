namespace FootballManager.Domain.Injuries;

/// <summary>The game's medical centre for the user's team: every first-team player's record and the season summary.</summary>
public class MedicalCentre
{
    public List<PlayerMedicalRecord> Players { get; set; } = [];
    public required SeasonInjurySummary Season { get; set; }
}
