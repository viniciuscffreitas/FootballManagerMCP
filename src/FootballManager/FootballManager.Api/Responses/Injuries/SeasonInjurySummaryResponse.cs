namespace FootballManager.Api.Responses.Injuries;

public class SeasonInjurySummaryResponse
{
    public int? InjuriesThisSeason { get; set; }

    /// <summary>E.g. "17 a mais".</summary>
    public string? ComparedWithLastSeason { get; set; }

    /// <summary>E.g. "68 a menos".</summary>
    public string? ExpectedOverSeason { get; set; }

    public string? LeagueAverage { get; set; }
    public int? SlightInjuries { get; set; }
    public int? MinorInjuries { get; set; }
    public int? ModerateInjuries { get; set; }
    public int? MajorInjuries { get; set; }
    public int? HighRiskPlayers { get; set; }
    public string? ExhaustedAndRecurring { get; set; }
    public string? ExpectedReturnsNextMatch { get; set; }
}
