namespace FootballManager.Domain.Board;

/// <summary>Where the user stands with the club: the board's and the supporters' confidence, the club vision and the board's dealings.</summary>
public class BoardStatus
{
    public required BoardConfidence Board { get; set; }
    public required SupporterConfidence Supporters { get; set; }

    /// <summary>The club vision's objective for the current season; null when the game shows none.</summary>
    public VisionObjective? ThisYearObjective { get; set; }

    /// <summary>The club vision's objective for its final year; null when the game shows none.</summary>
    public VisionObjective? FinalObjective { get; set; }

    /// <summary>What the supporters want from the club, one line each, e.g. "Jogar futebol ofensivo".</summary>
    public List<string> SupporterCulture { get; set; } = [];

    /// <summary>What the board wants from the club, one line each, as the game words it (its "none" text when it has none).</summary>
    public List<string> BoardCulture { get; set; } = [];

    /// <summary>Requests the user has made to the board.</summary>
    public int BoardRequests { get; set; }

    /// <summary>Requests to the board still open.</summary>
    public int ActiveBoardRequests { get; set; }

    /// <summary>Promises the user made to the board; empty when there are none.</summary>
    public List<string> Promises { get; set; } = [];
}
