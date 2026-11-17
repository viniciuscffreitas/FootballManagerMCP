namespace FootballManager.Domain.Board;

/// <summary>The supporters' confidence in the user, as the game's club vision screen words it.</summary>
public class SupporterConfidence
{
    /// <summary>The game's overall grade (3 seen with the supporters "contentes").</summary>
    public int? Grade { get; set; }

    public string? Summary { get; set; }
    public string? Matches { get; set; }
    public string? Vision { get; set; }
}
