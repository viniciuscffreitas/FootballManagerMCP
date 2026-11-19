namespace FootballManager.Domain.Registration;

/// <summary>A player's squad number on the game's squad numbers screen, and the numbers it offers him.</summary>
public class PlayerSquadNumber
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Null when he has none.</summary>
    public int? Number { get; set; }

    /// <summary>Whether the game lets the user change it now.</summary>
    public bool CanChange { get; set; }

    /// <summary>The game's suggestion for him; null when it has none.</summary>
    public int? Suggested { get; set; }

    public List<SquadNumberOption> Options { get; set; } = [];
}
