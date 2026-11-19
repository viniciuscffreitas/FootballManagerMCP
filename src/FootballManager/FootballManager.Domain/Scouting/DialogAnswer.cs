namespace FootballManager.Domain.Scouting;

/// <summary>The game's confirmation dialog, as the tool found it and what it did with it.</summary>
public class DialogAnswer
{
    public DialogState State { get; set; }

    /// <summary>The dialog's title and description, in the game's display language, when one showed.</summary>
    public string? Text { get; set; }
}
