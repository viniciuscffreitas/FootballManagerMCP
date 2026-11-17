using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class ContractsTools
{
    [McpServerTool(Name = "get_contracts")]
    [Description("The contracts of the user's first team in their running Football Manager 27 save, read live from the " +
                 "game. Read only. Returns one entry per player: FM id, name, wage as the game writes it with its " +
                 "period (e.g. \"R$80 mil p/s\"), endDate (yyyy-MM-dd, null without one), timeRemaining (e.g. \"2 anos, " +
                 "291 dias\"), type, agreedPlayingTime (the playing time promised, e.g. \"Goleiro titular\"), " +
                 "releaseClause (the minimum fee release clause as written, null when there is none), clausesCount, " +
                 "hasRenewalOffer, and staffAdvice (the staff's advice on renewing, e.g. \"Não oferecer contrato\"). " +
                 "Money is in the save's currency and texts in the game's display language. Fails with a 503 when the " +
                 "game isn't running or no save is loaded.")]
    public static async Task<string> GetContracts(
        IHttpClientFactory httpFactory,
        [Description("Optional, yyyy-MM-dd: only contracts ending on or before this day, e.g. the season's end. " +
                     "Contracts without an end date are left out.")]
        string? expiringBy = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var path = expiringBy is null ? "/api/v1/contracts" : $"/api/v1/contracts?expiringBy={Uri.EscapeDataString(expiringBy)}";
        var response = await http.GetAsync(path);
        return await response.ReadContentOrError();
    }
}
