namespace FootballManager.Api.Requests.Matches;

public class LiveRoleRequest
{
    /// <summary>The FM id of the player on the pitch.</summary>
    public int PlayerId { get; set; }

    /// <summary>The phase whose role changes: with the ball (true) or without it.</summary>
    public bool InPossession { get; set; } = true;

    /// <summary>The role key for that phase, e.g. InsideForward.</summary>
    public required string Role { get; set; }
}
