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
    [Description("Take the contract negotiation of a player whose club accepted the user's offer, through the game's " +
                 "contract panel and its own buttons, without the mouse. The panel opens from the acceptance's message " +
                 "(e.g. \"Proposta Santos por X aceita\", found in the inbox by the player's name) and has two pages: the " +
                 "agent's promises (\"Finalizar Promessas\" agrees to them), then the contract (reached with the panel's " +
                 "\"Negociar Contrato\", not the game's Continue), whose \"Fechar Acordo\" offers the deal at the terms " +
                 "it shows. By default a dry run that opens and presses nothing: with the panel already on the contract's " +
                 "page it shows its terms (salary, length, bonuses, release clause as the panel words them), otherwise " +
                 "the message it would open from. With apply=true the API first checks and copies the save (see the " +
                 "README's Safety), then opens the panel if needed, agrees to the promises and goes on to the contract, " +
                 "and stops there with its terms: show the user the salary and length, and once they agree call again " +
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
}
