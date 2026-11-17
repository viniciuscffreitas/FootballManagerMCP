namespace FootballManager.Domain.Matches;

/// <summary>The tactic the user's team is playing the match with.</summary>
public class LiveTactic
{
    public string? Name { get; set; }

    /// <summary>The shape with the ball, e.g. "4-2-3-1".</summary>
    public string? Formation { get; set; }

    /// <summary>In the game's display language, e.g. "Cautelosa".</summary>
    public string? Mentality { get; set; }
}
