namespace FootballManager.Domain.Contracts;

/// <summary>How much of a contract change reached the game.</summary>
public enum AppliedState
{
    /// <summary>Nothing was sent: a dry run.</summary>
    No,

    /// <summary>
    /// Part of it was sent: the menu entry was picked (or may have been) but the game's dialog wasn't answered, or the
    /// game didn't answer a send. The game may be waiting on the user: check it before sending the change again.
    /// </summary>
    Partial,

    /// <summary>The entry was picked and its dialog answered (or the game showed none).</summary>
    Yes
}
