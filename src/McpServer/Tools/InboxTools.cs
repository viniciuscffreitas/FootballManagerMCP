using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class InboxTools
{
    [McpServerTool(Name = "get_inbox")]
    [Description("Get the user's inbox from their running Football Manager 27 save, read live from the game, newest " +
                 "first: staff reports (injuries, fitness tests, training, scouting, opposition reports), the board, " +
                 "players asking to talk, press conferences, and press articles the staff forward. For each message: " +
                 "its id, date and time, sender, title, subtitle (what it's about), body, whether it's been read, and " +
                 "flags the game sets: isImportant, isPriority, canReply (the game offers answers to pick in the " +
                 "message), hasReplied, isForwarded (a press article; source is where it was published). Also the " +
                 "inbox's total and how many are unread. Text is in the game's display language. This only reads: " +
                 "it doesn't mark anything as read. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetInbox(
        IHttpClientFactory httpFactory,
        [Description("Only the unread messages.")]
        bool unreadOnly = false,
        [Description("At most this many messages (1-100), 20 by default. A message is about 300 bytes, more with articles.")]
        int? limit = null,
        [Description("Skip this many of the newest, to page back through older messages.")]
        int skip = 0,
        [Description("Include the text of the press articles staff forward (about 1 KB each).")]
        bool includeArticles = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = $"unreadOnly={unreadOnly.ToString().ToLowerInvariant()}" +
                    (limit is { } max ? $"&limit={max}" : string.Empty) +
                    $"&skip={skip}" +
                    $"&includeArticles={includeArticles.ToString().ToLowerInvariant()}";

        var response = await http.GetAsync($"/api/v1/inbox?{query}");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "get_message_options")]
    [Description("Get the answers the game offers to one inbox message (by id from get_inbox, e.g. one with canReply), " +
                 "read live without opening it: each option's index, title (in the game's language), the native event " +
                 "its button sends (27 is a plain reply that reply_to_message can send, with its responseId and " +
                 "responseValue), isDisabled, isRecommended (the game's recommendation), the confirmation text the " +
                 "game would show, and flow: what an option opens instead of answering (a panel by its newsAction, " +
                 "e.g. the board's vision negotiation, a conversation with a player, the tutorial). Also " +
                 "hasRequiredAction (this message needs an answer before Continue moves the calendar on) and " +
                 "requiredResponses: how many answers the whole inbox still needs before Continue moves on " +
                 "(ManagerResponseRequiredCount). Tell the user when that is above 0. This only reads. Not found when " +
                 "the inbox holds no message with that id. Fails with a 503 when the game isn't running or no save is " +
                 "loaded.")]
    public static async Task<string> GetMessageOptions(
        IHttpClientFactory httpFactory,
        [Description("The message's id, from get_inbox.")]
        int messageId)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync($"/api/v1/inbox/{messageId}/options");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "reply_to_message")]
    [Description("Answer an inbox message with one of its options, as the user would by pressing its button: the " +
                 "game's plain reply (event 27), for things like accepting the board's vision, the code of conduct, " +
                 "declining the assistant's introductions, a club's counter-offer. Read the options first with " +
                 "get_message_options and pick one by index. Prefer an option that hands the choice to the staff " +
                 "(\"Pedir a ...\", \"Delegar\") when the user has no view, and decline tutorials. An option the message " +
                 "doesn't have, a disabled one, and one that opens a flow (a panel, a conversation, the tutorial) are " +
                 "refused with the reason; a flow says what it opens. A message already answered is refused. By default " +
                 "a dry run that shows the option it would send and how many answers the inbox still needs: show the " +
                 "user and ask. With apply=true the API first checks and copies the save (see the README's Safety), then " +
                 "sends the answer and reads the message back: hasReplied, and requiredResponses after it. Refused " +
                 "while a match is being played.")]
    public static async Task<string> ReplyToMessage(
        IHttpClientFactory httpFactory,
        [Description("The message's id, from get_inbox.")]
        int messageId,
        [Description("The option's index, from get_message_options.")]
        int option,
        [Description("False (the default) only checks. True sends the answer: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsync(
            $"/api/v1/inbox/{messageId}/reply?option={option}&apply={apply.ToString().ToLowerInvariant()}", null);
        return await response.ReadContentOrError();
    }
}
