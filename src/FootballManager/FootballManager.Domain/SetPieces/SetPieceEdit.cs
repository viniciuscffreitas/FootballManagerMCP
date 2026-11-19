namespace FootballManager.Domain.SetPieces;

/// <summary>A change to the captaincy or the takers, or what a dry run says it would do.</summary>
public class SetPieceEdit
{
    /// <summary>
    /// Written once any change went into the game; NotWritten for a dry run or when none did; Unknown when a write's
    /// answer was lost and the read-back failed.
    /// </summary>
    public WriteState Applied { get; set; }

    /// <summary>Whether the captain change went into the game; NotWritten when none was asked or needed.</summary>
    public WriteState CaptainWritten { get; set; }

    /// <summary>Whether the vice-captain change went into the game; NotWritten when none was asked or needed.</summary>
    public WriteState ViceCaptainWritten { get; set; }

    /// <summary>True only when the read-back shows every role the call asked for; false for a dry run.</summary>
    public bool Confirmed { get; set; }

    /// <summary>The setup as the game held it before the call.</summary>
    public required SetPieceSetup Before { get; set; }

    /// <summary>The setup read back after the write; null for a dry run.</summary>
    public SetPieceSetup? After { get; set; }

    /// <summary>What the call does (or would do), in order.</summary>
    public List<string> Steps { get; set; } = [];

    public List<string> Notes { get; set; } = [];
}
