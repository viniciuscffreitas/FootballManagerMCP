namespace FootballManager.Domain.Transfers;

/// <summary>Whether a player exit's menu entry went into the game.</summary>
public enum PlayerExitWrite
{
    /// <summary>Not sent (also a dry run).</summary>
    NotWritten,

    /// <summary>The game took it, by its answer or by the read back.</summary>
    Written,

    /// <summary>Sent, but the game's answer was lost and the read back doesn't show the change: check with get_squad.</summary>
    Unknown
}
