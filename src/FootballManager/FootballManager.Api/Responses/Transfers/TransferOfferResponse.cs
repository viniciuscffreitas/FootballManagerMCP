namespace FootballManager.Api.Responses.Transfers;

public class TransferOfferResponse
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Club { get; set; }
    public long Fee { get; set; }
    public bool Applied { get; set; }

    /// <summary>The fee the panel started at, before its buttons moved it.</summary>
    public long? StartingFee { get; set; }

    /// <summary>What the director of football advised beside the offer, his suggested fee among it.</summary>
    public List<string>? Advice { get; set; }

    public bool Sent { get; set; }

    /// <summary>The texts the panel showed after the offer went.</summary>
    public List<string>? Response { get; set; }

    public List<string>? Notes { get; set; }
}
