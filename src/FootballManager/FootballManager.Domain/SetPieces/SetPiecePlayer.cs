namespace FootballManager.Domain.SetPieces;

/// <summary>A first-team player as the set piece screen names him.</summary>
public class SetPiecePlayer
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
