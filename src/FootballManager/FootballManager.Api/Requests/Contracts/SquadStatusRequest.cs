namespace FootballManager.Api.Requests.Contracts;

public class SquadStatusRequest
{
    /// <summary>FM id of one of the club's own players.</summary>
    public int PlayerId { get; set; }

    /// <summary>The squad status as the game's menu labels it, e.g. "Rotação".</summary>
    public required string Status { get; set; }
}
