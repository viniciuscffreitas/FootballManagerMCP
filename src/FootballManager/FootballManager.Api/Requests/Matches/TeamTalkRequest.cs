namespace FootballManager.Api.Requests.Matches;

public class TeamTalkRequest
{
    /// <summary>The line to say, by its index on the panel from 0; left out, the panel's lines are only read.</summary>
    public int? Line { get; set; }
}
