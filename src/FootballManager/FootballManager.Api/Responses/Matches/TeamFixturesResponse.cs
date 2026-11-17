namespace FootballManager.Api.Responses.Matches;

/// <summary>The user's next matches in every competition, in date order.</summary>
public class TeamFixturesResponse
{
    public required string TeamName { get; set; }
    public List<MatchSummaryResponse> Fixtures { get; set; } = [];
}
