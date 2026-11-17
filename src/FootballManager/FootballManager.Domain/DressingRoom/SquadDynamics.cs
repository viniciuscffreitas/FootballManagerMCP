namespace FootballManager.Domain.DressingRoom;

/// <summary>The dressing room as the game's squad dynamics screen shows it.</summary>
public class SquadDynamics
{
    public required SquadHierarchy Hierarchy { get; set; }
    public required DressingRoomAtmosphere Atmosphere { get; set; }

    /// <summary>The happiness issues open in the squad, each raised by one player.</summary>
    public List<HappinessIssue> Issues { get; set; } = [];

    /// <summary>The promises the user's manager has made to players and still has to keep.</summary>
    public List<PlayerPromise> Promises { get; set; } = [];

    /// <summary>Every first-team player, in the squad's order.</summary>
    public List<PlayerDynamics> Players { get; set; } = [];
}
