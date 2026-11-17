namespace FootballManager.Api.Responses.SquadDynamics;

public class AtmosphereResponse
{
    public string? Summary { get; set; }

    /// <summary>0-100.</summary>
    public int? Score { get; set; }

    public List<AtmosphereFactorResponse> Factors { get; set; } = [];
}
