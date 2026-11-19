namespace FootballManager.Domain.Scouting;

/// <summary>What became of the game's confirmation dialog after a menu entry was picked.</summary>
public enum DialogState
{
    /// <summary>No dialog showed: nothing was clicked.</summary>
    NotShown,

    /// <summary>A dialog showed but "Yes" wasn't clicked: it isn't the one the action opens, or it has no "Yes".</summary>
    NotAnswered,

    /// <summary>"Yes" was clicked and the dialog closed.</summary>
    Answered,

    /// <summary>"Yes" was sent but the game didn't answer, or the dialog stayed open: it may or may not have taken.</summary>
    Unknown
}
