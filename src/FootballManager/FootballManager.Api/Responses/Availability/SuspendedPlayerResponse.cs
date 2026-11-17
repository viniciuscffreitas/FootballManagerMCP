namespace FootballManager.Api.Responses.Availability;

public class SuspendedPlayerResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>Where the ban applies, e.g. "National league".</summary>
    public string? Scope { get; set; }

    public int? MatchesRemaining { get; set; }
    public int? DaysRemaining { get; set; }
    public string? Reason { get; set; }
}
