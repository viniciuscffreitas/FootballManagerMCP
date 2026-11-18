namespace FootballManager.Api.Requests.Contracts;

public class DofRenewalRequest
{
    /// <summary>FM id of one of the club's own players. Required: left out, it's a 400, never player 0.</summary>
    public int? PlayerId { get; set; }
}
