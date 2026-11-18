using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TransferTargetTools
{
    [McpServerTool(Name = "manage_transfer_target")]
    [Description("Change how the club pursues a player of another club, without the mouse, through his own menu in the " +
                 "running game (the right click on him): action WithdrawOffer (withdraw the transfer offer made for him " +
                 "that his club hasn't answered; the game's same entry withdraws a trial offer), MarkDofTarget (add him to " +
                 "the director of football's transfer targets) or UnmarkDofTarget (take him off them). The action is " +
                 "required. The game offers only what fits where he stands: an action his menu doesn't offer now is " +
                 "refused with where he stands, and nothing is sent. The player must be one the scouting tools, a team's " +
                 "squad or search_database have listed. By default a dry run that sends nothing: it returns choice (the " +
                 "menu's entry, as the game labels it), confirmation (what the game would ask), offerPending and " +
                 "isDofTarget (read from the same menu). With apply=true the API first checks and copies the save (see " +
                 "the README's Safety), reads the menu again and sends nothing if the entry changed, runs it and reads the " +
                 "menu back: write (Written, NotWritten or Unknown) and confirmed say whether the game took it. With " +
                 "applied true never send it again: a dry run shows where he stands. A loan offer, clauses or a player " +
                 "in exchange aren't made here yet: make_transfer_offer sends a plain transfer fee. Refused while a match " +
                 "is being played.")]
    public static async Task<string> ManageTransferTarget(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id: a player of another club.")]
        int playerId,
        [Description("WithdrawOffer, MarkDofTarget or UnmarkDofTarget.")]
        string action,
        [Description("False (the default) only checks. True runs the entry: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/transfers/target?apply={apply.ToString().ToLowerInvariant()}", new { playerId, action });

        return await response.ReadContentOrError();
    }
}
