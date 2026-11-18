namespace FootballManager.Domain.Contracts;

/// <summary>Whether the pick of the menu entry reached the game.</summary>
public enum MenuWrite
{
    /// <summary>Nothing was sent.</summary>
    NotWritten,

    /// <summary>The game took the pick.</summary>
    Written,

    /// <summary>The pick was sent but the game didn't answer, and the read back couldn't tell whether it took.</summary>
    Unknown
}
