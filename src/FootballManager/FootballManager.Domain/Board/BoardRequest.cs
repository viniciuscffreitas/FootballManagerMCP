using FootballManager.Domain.Scouting;

namespace FootballManager.Domain.Board;

/// <summary>A request to the board as planned or sent: the menu the game offers for it and the entry picked.</summary>
public class BoardRequest
{
    public bool Applied { get; set; }
    public BoardMenuAction Action { get; set; }

    /// <summary>Every request the board's menu offers now; one it won't hear (a request already open, say) is left out.</summary>
    public List<BoardMenuAction> Offered { get; set; } = [];

    /// <summary>The action's entries as the game offers them now, in its display language.</summary>
    public List<MenuEntry> Menu { get; set; } = [];

    /// <summary>The entry picked; null when the menu has several and none was named.</summary>
    public string? Choice { get; set; }

    /// <summary>What the game tells the user on sending it ("A diretoria entrará em contato ..."), when it does.</summary>
    public string? Confirmation { get; set; }

    /// <summary>After an apply: the talk with the board the entry opened, when it opens one; answer it with the board's conversation.</summary>
    public BoardCard? Card { get; set; }

    public List<string> Notes { get; set; } = [];
}
