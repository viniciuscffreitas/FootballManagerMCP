namespace FootballManager.Api.Responses.Matches;

public class LivePlayerResponse
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public int? Slot { get; set; }
    public double? Rating { get; set; }
    public int? Condition { get; set; }

    /// <summary>Flags are only sent when true.</summary>
    public bool? YellowCard { get; set; }

    public bool? RedCard { get; set; }
    public int? Goals { get; set; }
    public int? MinuteOn { get; set; }
}
