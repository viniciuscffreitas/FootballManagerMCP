namespace FootballManager.Domain.SetPieces;

/// <summary>A change to the captaincy or the takers, or what a dry run says it would do.</summary>
public class SetPieceEdit
{
    /// <summary>False for a dry run, which writes nothing.</summary>
    public bool Applied { get; set; }

    /// <summary>True once the game took the captain change; false when none was written.</summary>
    public bool CaptainWritten { get; set; }

    /// <summary>True once the game took the vice-captain change; false when none was written.</summary>
    public bool ViceCaptainWritten { get; set; }

    /// <summary>The setup as the game held it before the call.</summary>
    public required SetPieceSetup Before { get; set; }

    /// <summary>The setup read back after the write; null for a dry run.</summary>
    public SetPieceSetup? After { get; set; }

    /// <summary>What the call does (or would do), in order.</summary>
    public List<string> Steps { get; set; } = [];

    public List<string> Notes { get; set; } = [];
}
