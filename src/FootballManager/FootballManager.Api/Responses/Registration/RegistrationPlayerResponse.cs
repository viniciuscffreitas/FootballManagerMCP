namespace FootballManager.Api.Responses.Registration;

public class RegistrationPlayerResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? SquadNumber { get; set; }
    public bool IsGoalkeeper { get; set; }
}
