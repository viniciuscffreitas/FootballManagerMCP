using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Opposition;

/// <summary>A player of the expected lineup, where the game expects him and in which role (in possession).</summary>
public class ExpectedPlayer
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public TacticalPosition? Position { get; set; }

    /// <summary>The role's key, the game's English identifier (e.g. "BallPlayingGoalkeeper"), as get_tactics gives it.</summary>
    public string? RoleKey { get; set; }

    /// <summary>The duty the game pairs with the role (e.g. "Support"), when it gives one.</summary>
    public string? Duty { get; set; }
}
