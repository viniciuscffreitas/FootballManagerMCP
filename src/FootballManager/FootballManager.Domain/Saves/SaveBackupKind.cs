namespace FootballManager.Domain.Saves;

/// <summary>Who made a copy in the backup folder, which says whether the rotation may remove it.</summary>
public enum SaveBackupKind
{
    /// <summary>The guard's copy before a write; only the club's two newest are kept.</summary>
    BeforeWrite,

    /// <summary>The save as it was before a restore replaced it; never removed.</summary>
    BeforeRestore,

    /// <summary>Any other .fm file (the copy made before MCPFM, the game's or the user's); never removed.</summary>
    Other
}
