namespace FootballManager.Domain.DressingRoom;

/// <summary>A promise the user's manager made to a player, as the game's promises screen lists it.</summary>
public class PlayerPromise
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    /// <summary>How it is going, as the game words it.</summary>
    public string? Status { get; set; }

    public int? DaysRemaining { get; set; }

    /// <summary>The date the promise falls due, as the game sends it.</summary>
    public string? EndDate { get; set; }
}
