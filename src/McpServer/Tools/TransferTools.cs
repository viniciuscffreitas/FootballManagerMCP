using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TransferTools
{
    [McpServerTool(Name = "make_transfer_offer")]
    [Description("Make a transfer offer for a player of another club in the running game, without the mouse, through " +
                 "the game's transfer offer panel: it opens from his menu, its fee is moved with the panel's +/− buttons " +
                 "(R$10 mil a press in the game) to exactly the fee asked, and \"make offer\" sends it with the panel's " +
                 "other terms as they start (no instalments or clauses). The player must be one the scouting tools, " +
                 "get_competition_stats, a team's squad or search_database have listed. By default a dry run that opens " +
                 "nothing: it shows the player, his club and the fee; show the user and ask. With apply=true the API " +
                 "first checks and copies the save (see the README's Safety), opens the panel, moves the fee and sends it " +
                 "only if the panel then shows exactly that fee: a fee the buttons can't reach exactly (they go past it, " +
                 "or stop), or more than 100 presses from where the panel starts, is refused and nothing is sent. Returns " +
                 "startingFee (the panel's own starting value), advice (the director of football's lines beside the " +
                 "panel: his suggested fee and clauses), sent, and response (what the panel showed after). The club " +
                 "answers later, in the inbox: read it with get_inbox. A counter-offer (e.g. \"Proposta por X " +
                 "negociada\") is answered with get_message_options and reply_to_message (its plain replies, e.g. " +
                 "\"Aceitar\", are event 27); once the club accepts, negotiate_contract goes on to the player's contract. " +
                 "Use the fm27-guide skill's market rules (release clause, wage scale, contracts to December, sell-on) to " +
                 "choose the fee. Say to the user, before applying, not to click in the game for about a minute. A few " +
                 "seconds to open, about a second per press. Refused while a match is being played.")]
    public static async Task<string> MakeTransferOffer(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id.")]
        int playerId,
        [Description("The fee offered, in the game's currency units, e.g. 790000 for R$790 mil.")]
        long fee,
        [Description("False (the default) only checks. True opens the panel and sends the offer: only once the user has agreed to this fee.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/transfers/offer?apply={apply.ToString().ToLowerInvariant()}", new { playerId, fee });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "negotiate_contract")]
    [Description("Take the contract negotiation of a player whose club accepted the user's offer, or a new contract " +
                 "for one of the club's own players, through the game's contract panel and its own buttons, without the " +
                 "mouse. For a signing the panel opens from the acceptance's message (e.g. \"Proposta Santos por X " +
                 "aceita\", found in the inbox by the player's name); for an own player, from his menu (\"Iniciar " +
                 "negociação de contrato\"). It has two pages: the " +
                 "agent's promises (\"Finalizar Promessas\" agrees to them), then the contract (reached with the panel's " +
                 "\"Negociar Contrato\", not the game's Continue), whose \"Fechar Acordo\" offers the deal at the terms " +
                 "it shows. By default a dry run that opens and presses nothing: with the panel already on the contract's " +
                 "page it shows its terms (salary, length, bonuses, release clause as the panel words them), otherwise " +
                 "the message it would open from. With apply=true the API first checks and copies the save (see the " +
                 "README's Safety). From the message it opens the panel and stops on the agent's promises, to show the " +
                 "user; the next apply agrees to them, goes on to the contract and stops there with its terms: show the user the salary and length, and once they agree call again " +
                 "with apply=true to offer the deal (\"Fechar Acordo\" is only pressed when the panel was on the " +
                 "contract's page as the call began). Returns steps (the buttons pressed), terms, buttons, response (what " +
                 "the agent said), and closed once the deal was offered and the panel no longer asks for it. Changing " +
                 "the terms isn't supported: the deal goes at the agent's asking terms. Afterwards the transfer completes " +
                 "on the game's calendar; a message such as \"Transferência de X adiada\" (the selling club postpones it) " +
                 "is answered with get_message_options and reply_to_message (\"Aceitar Adiamento\"). Refused when " +
                 "neither the panel nor a message for that player is there, and when the open panel is another " +
                 "player's. Say to the user, before applying, not to click in the game for about 30 seconds. Refused " +
                 "while a match is being played.")]
    public static async Task<string> NegotiateContract(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id.")]
        int playerId,
        [Description("False (the default) only checks. True opens the panel and presses its buttons: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/transfers/contract?apply={apply.ToString().ToLowerInvariant()}", new { playerId });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "manage_player_exit")]
    [Description("Change how one of the club's own players may leave, without the mouse, through his own menu in the " +
                 "running game (the right click on him): action SetTransferStatus (the menu's statuses, e.g. \"Situação de " +
                 "transferência > Na lista de transferências\", \"Situação de empréstimo > Disponível para " +
                 "empréstimo\", and the answers to offers under \"Opções de Ofertas de Transferência\"), RemoveFromTransferList, RemoveFromLoanList, OfferToClubs, Recall (a " +
                 "player the club has out on loan comes back) or ExtendLoan. The game offers only what fits where he " +
                 "stands: an action its menu doesn't offer now is refused with where he stands, and nothing is sent. " +
                 "By default a dry run that sends nothing: it returns choices (every entry the menu offers for the " +
                 "action, labelled as the game labels each level, e.g. \"Transfer status > Transfer list\"), choice " +
                 "(the entry picked: the only one, or the one named), confirmation (what the game would ask), and " +
                 "status (transferListed, loanListed, outOnLoan, read from the same menu). When the menu has more than " +
                 "one entry, pass one of choices as choice. With apply=true the API first checks and copies the save " +
                 "(see the README's Safety), reads the menu again and runs the entry only if it is still there and not " +
                 "greyed out, then returns status as the game then shows him, applied (Written, NotWritten, or Unknown " +
                 "when the game's answer was lost and the read back doesn't show the change: check get_squad before sending it again) " +
                 "and confirmed (true when the read back shows the change asked for, false with a warning in notes " +
                 "when it doesn't, null when the menu can't show it: an offer to clubs, a loan extension, the answers " +
                 "to offers, a loan status off the loan list). A status he already has is refused, and nothing is sent. An entry " +
                 "that opens a screen of its own isn't run: the asking price dialog (\"Definir situação de " +
                 "transferência\") and OfferToClubs (\"Oferecer via TransferRoom\", the Transfer Room panel) are " +
                 "refused for now, so the asking price and the advert are set by the user in the game; an entry the " +
                 "game greys out (e.g. a recall it doesn't allow yet) is refused too. get_squad shows the listing " +
                 "among a player's statuses (TransferListed, ListedForLoan). Use the fm27-guide skill's market rules before listing a player. Refused while " +
                 "a match is being played.")]
    public static async Task<string> ManagePlayerExit(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id: one of the club's own players, or one it has out on loan.")]
        int playerId,
        [Description("SetTransferStatus, RemoveFromTransferList, RemoveFromLoanList, OfferToClubs, Recall or ExtendLoan.")]
        string action,
        [Description("The menu entry to pick, exactly as the dry run lists it in choices. Needed when there's more than one.")]
        string? choice = null,
        [Description("False (the default) only checks. True runs the entry: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/transfers/exit?apply={apply.ToString().ToLowerInvariant()}", new { playerId, action, choice });

        return await response.ReadContentOrError();
    }
}
