namespace FootballManager.Domain.Matches;

/// <summary>One of the user's players in the match being played.</summary>
public class LivePlayer
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }

    /// <summary>His place in the lineup: 0-10 on the pitch, -1 once he has left it.</summary>
    public int? Slot { get; set; }

    public double? Rating { get; set; }

    /// <summary>Physical condition, in percent.</summary>
    public int? Condition { get; set; }

    public bool YellowCard { get; set; }
    public bool RedCard { get; set; }
    public int? Goals { get; set; }

    /// <summary>When he came on, for a substitute.</summary>
    public int? MinuteOn { get; set; }

    /// <summary>When he left the pitch.</summary>
    public int? MinuteOff { get; set; }

    public bool WentOffInjured { get; set; }
}
