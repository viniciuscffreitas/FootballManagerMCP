using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Rotation;

/// <summary>One slot of the eleven whose starter goes out, and the reserve taking his place (if any qualifies).</summary>
public class RotationSwap
{
    /// <summary>The slot's index in the game's lineup, 0 to 10; it keeps its position and role.</summary>
    public int Index { get; set; }

    /// <summary>The slot's position in possession, e.g. "DCL".</summary>
    public string? Position { get; set; }

    /// <summary>The slot's role key in possession, which both ratings are in.</summary>
    public string? Role { get; set; }

    public int OutPlayerId { get; set; }
    public required string OutName { get; set; }
    public Rating? OutRating { get; set; }

    /// <summary>Why he goes out: injured, suspended, or tired.</summary>
    public required string Reason { get; set; }

    public int? InPlayerId { get; set; }
    public string? InName { get; set; }
    public Rating? InRating { get; set; }
    public int? InCondition { get; set; }
    public int? InMatchSharpness { get; set; }

    /// <summary>Why the reserve, or why nobody: what the reader should weigh.</summary>
    public string? Detail { get; set; }
}
