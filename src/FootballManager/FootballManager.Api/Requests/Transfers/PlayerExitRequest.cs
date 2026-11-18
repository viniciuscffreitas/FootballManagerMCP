using FootballManager.Domain.Transfers;

namespace FootballManager.Api.Requests.Transfers;

public class PlayerExitRequest
{
    /// <summary>Required: the FM id of one of the club's own players, or of one it has out on loan. Left out, it is refused.</summary>
    public int? PlayerId { get; set; }

    /// <summary>Required: left out, it is refused rather than read as the enum's first value (SetTransferStatus).</summary>
    public PlayerExitAction? Action { get; set; }

    /// <summary>The menu entry to pick, as the dry run lists it in choices; needed when the menu has more than one.</summary>
    public string? Choice { get; set; }
}
