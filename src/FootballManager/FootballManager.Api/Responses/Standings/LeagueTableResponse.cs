namespace FootballManager.Api.Responses.Standings;

public class LeagueTableResponse
{
    public string? League { get; set; }
    public required string TeamName { get; set; }

    /// <summary>The user's place in the table; null when there's no table yet.</summary>
    public int? Position { get; set; }

    /// <summary>Where the press expects the user's team to finish.</summary>
    public int? MediaExpectedPosition { get; set; }

    /// <summary>One per team, top of the table first.</summary>
    public List<LeagueTableRowResponse> Rows { get; set; } = [];

    /// <summary>Why the table is empty; null when it has rows.</summary>
    public string? Reason { get; set; }
}
