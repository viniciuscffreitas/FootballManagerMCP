using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

public class ScoutedPlayerListResponse
{
    public ScoutingSource Source { get; set; }
    public RecruitmentFocusResponse? Focus { get; set; }
    public FocusPool? Pool { get; set; }

    /// <summary>The search screen's state, for the player search.</summary>
    public PlayerSearchStatusResponse? Search { get; set; }

    /// <summary>The players listed here.</summary>
    public int Count { get; set; }

    /// <summary>How many the list holds; more than <see cref="Count"/> when a limit cut it short.</summary>
    public int Total { get; set; }

    /// <summary>With a performance filter: how many players were read and cut down to the ones listed.</summary>
    public int? Scanned { get; set; }

    /// <summary>With a performance filter: true when the whole list was read, false when some players were left unread.</summary>
    public bool? Complete { get; set; }

    /// <summary>With a performance filter: each league the players read play in, with how many of them do.</summary>
    public Dictionary<string, int>? Leagues { get; set; }

    /// <summary>With a league filter: the leagues asked for that no player read plays in.</summary>
    public List<string>? UnmatchedLeagues { get; set; }

    public bool? HiddenDataMasked { get; set; }
    public List<ScoutedPlayerResponse> Players { get; set; } = [];
}
