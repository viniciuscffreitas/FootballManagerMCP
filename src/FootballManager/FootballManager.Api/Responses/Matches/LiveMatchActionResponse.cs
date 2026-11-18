namespace FootballManager.Api.Responses.Matches;

public class LiveMatchActionResponse
{
    /// <summary>Substitution, Mentality, Role or TeamTalk.</summary>
    public required string Control { get; set; }

    public bool Applied { get; set; }
    public string? Change { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }

    /// <summary>The team talk's lines on screen now.</summary>
    public List<string>? Lines { get; set; }

    public string? Said { get; set; }
    public List<string>? Reaction { get; set; }

    /// <summary>The match read again after the write, as get_live_match answers.</summary>
    public LiveMatchResponse? Match { get; set; }

    public List<string>? Notes { get; set; }
}
