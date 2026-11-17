using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class ConversationTools
{
    [McpServerTool(Name = "get_player_conversations")]
    [Description("The one-to-one conversations a first-team player's menu offers now in the running game, by the game's " +
                 "names: PraisePlayerTrainingDevelopment / WarnPlayerTrainingDevelopment (praise or criticise his " +
                 "training), PraisePlayerPerformance / WarnPlayerPerformance (his recent performances), " +
                 "PraisePlayerConduct / WarnPlayerConduct (his conduct). Only reads. A conversation leaves the list " +
                 "once used, even when its card was closed without an answer, and some depend on where he " +
                 "stands (an injured player can't be praised for his performances). Player by FM id from get_squad. " +
                 "Use the fm27-guide skill to judge when to praise or criticise.")]
    public static async Task<string> GetPlayerConversations(
        IHttpClientFactory httpFactory,
        [Description("The first-team player's FM id.")]
        int playerId)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync($"/api/v1/conversations/{playerId}");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "talk_to_player")]
    [Description("Have a one-to-one conversation with a first-team player in the running game, without the mouse: opens " +
                 "it from his menu (one of get_player_conversations' list; anything else is refused), says the line " +
                 "chosen in each round, reads his reaction and ends the conversation. Each round offers a few lines; a " +
                 "criticism has a second round whose lines back down, convince or threaten. Opening the card uses the " +
                 "conversation up, even if it's closed without an answer, so by default this is a dry run that opens " +
                 "nothing: tell the user which conversation it opens and ask. With apply=true: the save is checked and " +
                 "copied first, the card opens, and the lines at choices (their index in each round, from 0) are said " +
                 "one per round. Without choices (or with fewer than rounds), the card stays open and the answer's " +
                 "lines are the round waiting: pick one and call again with continue=true, apply=true and choices. " +
                 "continue=true with apply=false only reads the open card. Returns each round's lines, the line said " +
                 "(chosenLine), the player's reaction (the texts the card showed after it) and body language, and " +
                 "ended once the card is closed. Texts are in the game's display language, cut at 80 characters. Say " +
                 "to the user, before applying, not to click in the game for about 30 seconds. A few seconds per round, " +
                 "plus a few to open. Refused while a match is being played.")]
    public static async Task<string> TalkToPlayer(
        IHttpClientFactory httpFactory,
        [Description("The first-team player's FM id.")]
        int playerId,
        [Description("The conversation to open, e.g. PraisePlayerTrainingDevelopment; not needed with continue=true.")]
        string? conversation = null,
        [Description("The line to say in each round, by index from 0, e.g. [1] for a praise or [0, 2] for a criticism and its second round.")]
        List<int>? choices = null,
        [Description("True says the choices on the conversation card already open (from a call without choices) instead of opening one.")]
        bool @continue = false,
        [Description("False (the default) only checks. True opens and talks: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/conversations?apply={apply.ToString().ToLowerInvariant()}",
            new { playerId, conversation, choices = choices ?? [], @continue });

        return await response.ReadContentOrError();
    }
}
