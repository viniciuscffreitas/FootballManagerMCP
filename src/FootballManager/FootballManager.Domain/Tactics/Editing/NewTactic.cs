namespace FootballManager.Domain.Tactics.Editing;

/// <summary>
/// A tactic to create in an empty slot, by key, as the game's tactic creator asks for it: a style, then a formation
/// with the ball and one without it.
/// </summary>
public class NewTactic
{
    /// <summary>The empty slot to create it in, 1 to 3. Creating makes it the slot the team plays.</summary>
    public int Slot { get; set; }

    public string? Style { get; set; }
    public ByPhase<string> Formation { get; set; } = new();
}
