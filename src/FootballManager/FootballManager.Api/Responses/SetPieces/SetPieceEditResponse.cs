namespace FootballManager.Api.Responses.SetPieces;

public class SetPieceEditResponse
{
    public bool Applied { get; set; }
    public bool CaptainWritten { get; set; }
    public bool ViceCaptainWritten { get; set; }

    /// <summary>True only when the read-back shows every role asked for; see notes when false after an apply.</summary>
    public bool Confirmed { get; set; }
    public required SetPieceSetupResponse Before { get; set; }

    /// <summary>The setup read back after the write; null for a dry run.</summary>
    public SetPieceSetupResponse? After { get; set; }

    public List<string>? Steps { get; set; }
    public List<string>? Notes { get; set; }
}
