namespace FootballManager.Domain.Board;

/// <summary>The board's confidence in the user, as the game's club vision screen words it.</summary>
public class BoardConfidence
{
    /// <summary>The game's overall grade (6 seen with the board "contente").</summary>
    public int? Grade { get; set; }

    public string? Summary { get; set; }
    public string? Matches { get; set; }
    public string? SquadSupport { get; set; }
    public string? Tactics { get; set; }
    public string? Vision { get; set; }
    public string? Transfers { get; set; }

    /// <summary>What the board praises, e.g. a convincing win.</summary>
    public List<string> Highlights { get; set; } = [];

    /// <summary>What worries the board, e.g. a defeat.</summary>
    public List<string> Criticism { get; set; } = [];
}
