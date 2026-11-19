namespace FootballManager.Domain.SetPieces;

/// <summary>What the player's menu offers now about the armband.</summary>
public class CaptaincyOffer
{
    /// <summary>"Tornar Capitão": the current captain's menu doesn't have it.</summary>
    public bool MakeCaptain { get; set; }

    /// <summary>"Tornar Vice-capitão": the current vice's menu doesn't have it.</summary>
    public bool MakeViceCaptain { get; set; }
}
