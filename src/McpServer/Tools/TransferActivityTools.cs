using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TransferActivityTools
{
    [McpServerTool(Name = "get_transfer_activity")]
    [Description("The user's transfers under way and the offers his club has received in their running Football " +
                 "Manager 27 save, as the game's transfer activity screen shows them, read live. Read only. Returns: " +
                 "counts (players with offers: all, in, out; offers awaiting the user's answer: all, in, out; trade " +
                 "offers); incomingValue and outgoingValue (what the bids would spend and bring in, guaranteed then " +
                 "potential, e.g. \"R$0 (R$0)\"); and negotiations, one per offer, those awaiting the user first: player " +
                 "FM id and name, direction (In: the club bids for him; Out: another club bids for one of the user's " +
                 "players), club (the other club of the deal), type, fee, wage, status and requiresResponse. Empty when " +
                 "nothing is under way. Texts and money come as the game shows them, in its display language and " +
                 "currency. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetTransferActivity(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/transfer-activity");
        return await response.ReadContentOrError();
    }
}
