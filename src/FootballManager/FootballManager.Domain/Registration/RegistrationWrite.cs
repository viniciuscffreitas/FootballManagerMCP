namespace FootballManager.Domain.Registration;

/// <summary>What is known of a write sent to the game.</summary>
public enum RegistrationWrite
{
    /// <summary>Nothing was sent (also a dry run), or the game refused it before writing.</summary>
    NotWritten,

    Written,

    /// <summary>Sent, but the game's answer didn't arrive: the read back decides.</summary>
    Unknown
}
