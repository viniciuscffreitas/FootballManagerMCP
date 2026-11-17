namespace FootballManager.Domain.Matches;

/// <summary>
/// What the game shows of the match being played, as read: both sides, the goals already from the user's side, and
/// every player of the user's who has been on the pitch.
/// </summary>
public class LiveMatchReadout
{
    public int? Minute { get; set; }
    public string? Clock { get; set; }
    public string? Competition { get; set; }
    public required MatchSide Home { get; set; }
    public required MatchSide Away { get; set; }

    /// <summary>The user's team, which decides which side is "us".</summary>
    public int TeamId { get; set; }

    public TeamMatchStats? HomeStats { get; set; }
    public TeamMatchStats? AwayStats { get; set; }
    public List<MatchGoal> Goals { get; set; } = [];
    public List<LivePlayer> Players { get; set; } = [];
    public LiveTactic? Tactic { get; set; }

    /// <summary>As the game counts them; null when it doesn't say.</summary>
    public int? SubstitutionsRemaining { get; set; }

    public int? SubstitutionsAllowed { get; set; }

    public bool IsHome => Home.TeamId == TeamId;
}
