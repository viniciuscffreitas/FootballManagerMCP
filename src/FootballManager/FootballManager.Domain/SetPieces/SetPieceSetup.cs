namespace FootballManager.Domain.SetPieces;

/// <summary>Who wears the armband and who takes each set piece, as the game holds it now.</summary>
public class SetPieceSetup
{
    public SetPiecePlayer? Captain { get; set; }
    public SetPiecePlayer? ViceCaptain { get; set; }

    /// <summary>Who takes the armband when the captain and the vice are both off the pitch, in order.</summary>
    public List<SetPiecePlayer> CaptainStandIns { get; set; } = [];

    public List<SetPieceTakerList> TakerLists { get; set; } = [];

    /// <summary>The first team, to check the players a change names.</summary>
    public List<SetPiecePlayer> Squad { get; set; } = [];
}
