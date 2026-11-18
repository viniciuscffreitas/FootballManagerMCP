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

    public List<SetPiecePlayer> Takers { get; set; } = [];
}
