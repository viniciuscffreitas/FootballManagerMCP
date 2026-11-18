namespace FootballManager.Api.Requests.Contracts;

public class SquadStatusRequest
{
    /// <summary>FM id of one of the club's own players. Required: left out, it's a 400, never player 0.</summary>
    public int? PlayerId { get; set; }

    /// <summary>The squad status as the game's menu labels it, e.g. "Rotação".</summary>
    public string? Status { get; set; }
}
