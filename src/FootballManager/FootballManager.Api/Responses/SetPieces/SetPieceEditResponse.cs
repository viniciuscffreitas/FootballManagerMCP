using FootballManager.Domain.SetPieces;

namespace FootballManager.Api.Responses.SetPieces;

public class SetPieceEditResponse
{
    /// <summary>Written, NotWritten (also a dry run) or Unknown: then check with get_set_pieces before trying again.</summary>
    public WriteState Applied { get; set; }
    public WriteState CaptainWritten { get; set; }
    public WriteState ViceCaptainWritten { get; set; }

    /// <summary>True only when the read-back shows every role asked for; see notes when false after an apply.</summary>
    public bool Confirmed { get; set; }
    public required SetPieceSetupResponse Before { get; set; }

    /// <summary>The setup read back after the write; null for a dry run.</summary>
    public SetPieceSetupResponse? After { get; set; }

    public List<string>? Steps { get; set; }
    public List<string>? Notes { get; set; }
}
