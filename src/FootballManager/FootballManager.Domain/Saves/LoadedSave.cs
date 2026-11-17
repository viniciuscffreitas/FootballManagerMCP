namespace FootballManager.Domain.Saves;

/// <summary>The save the game has open, as the bridge sees it.</summary>
public class LoadedSave
{
    /// <summary>False on the start screen, and while a save is still loading.</summary>
    public bool Loaded { get; set; }

    /// <summary>A load is under way: the save isn't open yet.</summary>
    public bool Loading { get; set; }

    /// <summary>The save's file name without ".fm", e.g. "Vinicius Freitas - Santos".</summary>
    public string? Name { get; set; }

    /// <summary>The copy of the user's career kept for testing (the plugin's investigation save).</summary>
    public bool IsTestSave { get; set; }

    /// <summary>The user's club, read only once the save is loaded.</summary>
    public string? Club { get; set; }

    /// <summary>The game's current date, read only once the save is loaded.</summary>
    public DateOnly? GameDate { get; set; }
}
