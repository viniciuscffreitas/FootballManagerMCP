namespace FootballManager.Domain.DressingRoom;

/// <summary>A first-team player's place in the dressing room and what makes him happy or unhappy.</summary>
public class PlayerDynamics
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>His level in the hierarchy, as the game words it, e.g. "Líder de Time".</summary>
    public string? HierarchyLevel { get; set; }

    public List<string> HappinessPositives { get; set; } = [];
    public List<string> HappinessNegatives { get; set; } = [];

    /// <summary>How many promises the manager has made him.</summary>
    public int Promises { get; set; }
}
