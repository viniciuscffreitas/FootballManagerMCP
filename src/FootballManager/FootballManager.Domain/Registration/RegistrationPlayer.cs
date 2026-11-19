namespace FootballManager.Domain.Registration;

public class RegistrationPlayer
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>His squad number; null when he has none.</summary>
    public int? SquadNumber { get; set; }

    public bool IsGoalkeeper { get; set; }
}
