using System.Text.Json.Serialization;
using FootballManager.Domain.Transfers;

namespace FootballManager.Api.Responses.Transfers;

public class TransferTargetResponse
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Club { get; set; }
    public TransferTargetAction Action { get; set; }

    /// <summary>The menu entry run (or planned), labelled as the game labels it.</summary>
    public string? Choice { get; set; }

    /// <summary>What the game asks to confirm before running the entry, when it asks.</summary>
    public string? Confirmation { get; set; }

    public bool Applied { get; set; }

    /// <summary>NotWritten, Written or Unknown.</summary>
    public WriteState Write { get; set; }

    /// <summary>The menu read back shows the change; null when nothing was sent or it couldn't be read back.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Confirmed { get; set; }

    public bool OfferPending { get; set; }
    public bool IsDofTarget { get; set; }
    public List<string>? Notes { get; set; }
}
