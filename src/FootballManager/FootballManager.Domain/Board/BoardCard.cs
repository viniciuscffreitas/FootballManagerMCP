namespace FootballManager.Domain.Board;

/// <summary>A talk with the board open on screen (a request, the club vision or the season expectations meeting).</summary>
public class BoardCard
{
    /// <summary>The lines the user can say now, in the game's display language, in the order the card shows them.</summary>
    public List<string> Lines { get; set; } = [];
}
