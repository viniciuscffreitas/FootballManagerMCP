namespace FootballManager.Api.Responses.Transfers;

public class TransferOfferCountsResponse
{
    public int All { get; set; }
    public int In { get; set; }
    public int Out { get; set; }
    public int AwaitingResponseAll { get; set; }
    public int AwaitingResponseIn { get; set; }
    public int AwaitingResponseOut { get; set; }
    public int Trades { get; set; }
}
