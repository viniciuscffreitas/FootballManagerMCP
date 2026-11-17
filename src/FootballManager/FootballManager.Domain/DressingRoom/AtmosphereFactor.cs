namespace FootballManager.Domain.DressingRoom;

/// <summary>A reason the game gives for the atmosphere, with the weight it shows next to it.</summary>
public class AtmosphereFactor
{
    public required string Text { get; set; }

    /// <summary>The game's weight: 2 for the positive reasons seen, 0 for the negative one.</summary>
    public int? Value { get; set; }
}
