using FootballManager.Domain.Board;

namespace FootballManager.Api.Requests.Board;

public class BoardRequestRequest
{
    public BoardMenuAction Action { get; set; }

    /// <summary>The menu entry to pick, by its label as the dry run lists them; needed when the menu has several.</summary>
    public string? Entry { get; set; }
}
