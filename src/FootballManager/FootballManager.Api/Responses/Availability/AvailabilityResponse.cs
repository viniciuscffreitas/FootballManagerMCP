namespace FootballManager.Api.Responses.Availability;

public class AvailabilityResponse
{
    public required string TeamName { get; set; }
    public required NextMatchRulesResponse NextMatch { get; set; }

    /// <summary>Players an injury keeps out.</summary>
    public List<InjuredPlayerResponse> Injured { get; set; } = [];

    /// <summary>Every ban the team's players are serving, whatever the competition.</summary>
    public List<SuspendedPlayerResponse> Suspended { get; set; } = [];

    public List<PlayerAvailabilityResponse> Players { get; set; } = [];
}
