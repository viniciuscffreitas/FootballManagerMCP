namespace FootballManager.Api.Requests.Matches;

public class LiveMentalityRequest
{
    /// <summary>The mentality key, e.g. Attacking.</summary>
    public required string Mentality { get; set; }
}
