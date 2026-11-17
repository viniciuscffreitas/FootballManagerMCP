namespace FootballManager.Domain.DressingRoom;

/// <summary>The club atmosphere: the game's verdict, its 0-100 score, and the reasons it gives.</summary>
public class DressingRoomAtmosphere
{
    /// <summary>The verdict in a few words, e.g. "Muito bom".</summary>
    public string? Summary { get; set; }

    /// <summary>The atmosphere on the game's 0-100 scale.</summary>
    public int? Score { get; set; }

    public List<AtmosphereFactor> Factors { get; set; } = [];
}
