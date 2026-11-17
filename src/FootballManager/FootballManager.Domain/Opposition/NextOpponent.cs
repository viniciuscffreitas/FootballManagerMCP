namespace FootballManager.Domain.Opposition;

/// <summary>The user's next opponent, with the game's assessment of the threat it poses.</summary>
public sealed record NextOpponent(int TeamId, string TeamName, string? ThreatLevel);
