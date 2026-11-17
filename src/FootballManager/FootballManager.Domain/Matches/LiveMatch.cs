namespace FootballManager.Domain.Matches;

/// <summary>
/// The user's match while it's being played, from their side, as the match screen shows it. Out of a match only
/// <see cref="InProgress"/> and <see cref="Message"/> are set.
/// </summary>
public class LiveMatch
{
    public const string NoMatchMessage = "nenhuma partida em andamento";

    public bool InProgress { get; set; }

    /// <summary>Why there's nothing else, when no match is being played.</summary>
    public string? Message { get; set; }

    /// <summary>The minute being played.</summary>
    public int? Minute { get; set; }

    /// <summary>The match clock as the game writes it.</summary>
    public string? Clock { get; set; }

    public string? Competition { get; set; }
    public string? Opponent { get; set; }
    public Venue? Venue { get; set; }
    public int? GoalsFor { get; set; }
    public int? GoalsAgainst { get; set; }
    public List<MatchGoal> Goals { get; set; } = [];
    public TeamMatchStats? TeamStats { get; set; }
    public TeamMatchStats? OpponentStats { get; set; }

    /// <summary>The user's players on the pitch now, in lineup order.</summary>
    public List<LivePlayer> OnPitch { get; set; } = [];

    public List<MatchSubstitution> Substitutions { get; set; } = [];
    public int? SubstitutionsRemaining { get; set; }
    public LiveTactic? Tactic { get; set; }
}
