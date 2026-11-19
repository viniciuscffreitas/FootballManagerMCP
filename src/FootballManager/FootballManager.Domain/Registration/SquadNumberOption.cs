namespace FootballManager.Domain.Registration;

/// <summary>A squad number as the game's squad numbers screen offers it to a player.</summary>
public class SquadNumberOption
{
    public int Number { get; set; }

    /// <summary>The game's label: the number, then who wears it, e.g. "10. Neymar".</summary>
    public string? Label { get; set; }

    /// <summary>The game greys it out: someone else wears it.</summary>
    public bool Taken { get; set; }
}
