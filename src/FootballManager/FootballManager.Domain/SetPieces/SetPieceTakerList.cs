namespace FootballManager.Domain.SetPieces;

/// <summary>
/// One of the set piece screen's taker lists (penalties, a corner side, a kind of free kick), in the order the game
/// tries them: the first takes it, the next one when he's off the pitch.
/// </summary>
public class SetPieceTakerList
{
    /// <summary>The list's key, as <c>set_set_piece_takers</c> takes it.</summary>
    public required string Key { get; set; }

    /// <summary>The list's name in the game's display language.</summary>
    public string? Label { get; set; }

    /// <summary>
    /// For a left or right list: false until the game's side values (0 and 1) are checked against the set piece screen,
    /// so "left" may be the screen's right. Null for a list without a side.
    /// </summary>
    public bool? SideVerified { get; set; }

    public List<SetPiecePlayer> Takers { get; set; } = [];
}
