namespace FootballManager.Api.Requests.Matches;

public class SubstitutionRequest
{
    /// <summary>The FM id of the player on the pitch who goes off.</summary>
    public int PlayerOffId { get; set; }

    /// <summary>The FM id of the substitute on the bench who comes on.</summary>
    public int PlayerOnId { get; set; }
}
