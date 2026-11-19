namespace FootballManager.Domain.Board;

/// <summary>A line said in a talk with the board, as planned or sent.</summary>
public class BoardAnswer
{
    public bool Applied { get; set; }

    /// <summary>The talk as it stood before the line.</summary>
    public required BoardCard Card { get; set; }

    /// <summary>The line picked; null when none was named.</summary>
    public string? Line { get; set; }

    /// <summary>After an apply: the talk once the board answered; null when it closed.</summary>
    public BoardCard? After { get; set; }

    public List<string> Notes { get; set; } = [];
}
