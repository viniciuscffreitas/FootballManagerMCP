namespace FootballManager.Api.Requests.Transfers;

public class MakeTransferOfferRequest
{
    /// <summary>The player's FM id.</summary>
    public int PlayerId { get; set; }

    /// <summary>The fee offered, in the game's currency units (e.g. 790000 for R$790 mil).</summary>
    public long Fee { get; set; }
}
