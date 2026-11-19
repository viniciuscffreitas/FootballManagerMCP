namespace FootballManager.Domain.Registration;

/// <summary>The first team's registration in each competition that asks for one, as the game's registration screen shows it.</summary>
public class SquadRegistration
{
    public List<CompetitionRegistration> Competitions { get; set; } = [];
}
