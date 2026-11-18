namespace FootballManager.Domain.SetPieces;

/// <summary>Whether a write went into the game.</summary>
public enum WriteState
{
    /// <summary>Not sent, refused by the game, or the read-back shows it didn't take.</summary>
    NotWritten,

    /// <summary>The game took it, by its answer or by the read-back.</summary>
    Written,

    /// <summary>Sent, but the game's answer was lost and the read-back failed too: check with get_set_pieces.</summary>
    Unknown
}
