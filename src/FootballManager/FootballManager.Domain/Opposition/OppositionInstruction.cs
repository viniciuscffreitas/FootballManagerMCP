namespace FootballManager.Domain.Opposition;

/// <summary>The four instructions the game gives against one opponent player.</summary>
public enum OppositionInstruction
{
    /// <summary>Tight marking: none, never or always.</summary>
    Marking,

    /// <summary>Pressing him when he gets the ball: none, never or always.</summary>
    Pressing,

    /// <summary>How hard to tackle him: none, normal, soft or hard.</summary>
    Tackling,

    /// <summary>Which foot to show him onto: none, left, right or weaker.</summary>
    ShowOntoFoot
}
