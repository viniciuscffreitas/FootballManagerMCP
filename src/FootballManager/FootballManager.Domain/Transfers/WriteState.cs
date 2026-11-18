namespace FootballManager.Domain.Transfers;

/// <summary>Whether a change reached the game, as far as the call could tell.</summary>
public enum WriteState
{
    /// <summary>Nothing was sent: a dry run, or a refusal before the write.</summary>
    NotWritten,

    /// <summary>The game took the change.</summary>
    Written,

    /// <summary>The write was sent but the game didn't answer, and the read back couldn't tell whether it took.</summary>
    Unknown
}
