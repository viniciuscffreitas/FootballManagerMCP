namespace FootballManager.Api.Responses.Matches;

public class LiveMatchResponse
{
    public bool InProgress { get; set; }

    /// <summary>Only sent when no match is being played.</summary>
    public string? Message { get; set; }

    public int? Minute { get; set; }
    public string? Clock { get; set; }
    public string? Competition { get; set; }
    public string? Opponent { get; set; }
    public string? Venue { get; set; }
    public int? GoalsFor { get; set; }
    public int? GoalsAgainst { get; set; }
    public List<MatchGoalResponse>? Goals { get; set; }
    public TeamMatchStatsResponse? TeamStats { get; set; }
    public TeamMatchStatsResponse? OpponentStats { get; set; }
    public List<LivePlayerResponse>? OnPitch { get; set; }
    public List<MatchSubstitutionResponse>? Substitutions { get; set; }
    public int? SubstitutionsRemaining { get; set; }
    public LiveTacticResponse? Tactic { get; set; }
}
