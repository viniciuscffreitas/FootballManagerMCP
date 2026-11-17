using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Messages;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Messages;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's inbox through the plugin, a page at a time, as the portal lists it, and answers its messages the
// way their buttons do.
internal sealed class InboxBridgeClient(GameBridge bridge) : IInboxGameClient
{
    public async Task<Inbox> GetInboxAsync(
        bool unreadOnly, int skip, int take, bool includeArticles, CancellationToken cancellationToken = default)
    {
        var list = unreadOnly
            ? BridgeQuery.FromHuman().List(InboxConstants.UnreadMessages)
            : BridgeQuery.FromHuman()
                .List(InboxConstants.Messages)
                .Context(InboxConstants.Tab, InboxConstants.AllMessagesTab)
                .Context(InboxConstants.SearchText, string.Empty);

        var paths = includeArticles ? [.. InboxConstants.MessageProperties, InboxConstants.Article] : InboxConstants.MessageProperties;

        // A message without a sender (a news round-up) or a source (one not forwarded) leaves those out.
        var messages = bridge.GetListAsync<BridgeInboxMessageDto>(
            list.WithHumanAndTeam().Skip(skip).Take(take).Props(paths), cancellationToken, gapsExpected: true);
        var unread = bridge.GetRecordAsync<BridgeUnreadCountDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props([InboxConstants.UnreadCount]), cancellationToken);

        await Task.WhenAll(messages, unread);

        var page = await messages;

        return new Inbox
        {
            Total = page.Total ?? page.Count,
            Unread = (await unread).Values.Unread,
            Messages = page.Items.Select(message => message.Values).ToList().Adapt<List<InboxMessage>>()
        };
    }

    public async Task<MessageOptions?> GetMessageOptionsAsync(int messageId, CancellationToken cancellationToken = default)
    {
        var message = bridge.GetRecordAsync<BridgeMessageOptionsDto>(
            OnMessage(messageId).Props(InboxConstants.OptionProperties), cancellationToken, rejectionThrows: true);
        var required = RequiredResponsesAsync(cancellationToken);

        BridgeMessageOptionsDto values;

        try
        {
            values = (await message).Values;
        }
        catch (GameRejectedException)
        {
            // The plugin refuses a selector no item of the list matches: no message has that id.
            return null;
        }

        var actions = values.Actions;
        var options = (actions?.ActionsForButtonsList ?? []).Adapt<List<MessageOption>>();

        for (var i = 0; i < options.Count; i++)
        {
            options[i].Index = i;
            options[i].IsRecommended |= actions?.RecommendedActionIndex == i;
        }

        return new MessageOptions
        {
            MessageId = values.Id ?? messageId,
            Title = values.Title ?? string.Empty,
            CanReply = values.CanReply == true,
            HasReplied = values.HasReplied == true,
            HasRequiredAction = actions?.HasRequiredAction == true,
            Options = options,
            RequiredResponses = await required
        };
    }

    public Task SendReplyAsync(int messageId, int responseId, int? responseValue, CancellationToken cancellationToken = default)
    {
        // The record a reply button carries, field by field as the game's own click sends it (EventFmxNewsResponseAction).
        var data = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["$type"] = "Record",
            ["NPC"] = InboxConstants.ReplySender,
            ["ActionResponseId"] = responseId,
            ["ActionResponseValue"] = responseValue,
            ["ActionID"] = InboxConstants.NoActionId,
            ["ActionProps"] = new Dictionary<string, object>(),
            ["ActionData"] = new Dictionary<string, object>()
        });

        return bridge.WriteAsync<JsonElement>(
            OnMessage(messageId).ToWrite("/native",
                ("event", InboxConstants.ReplyEvent), ("field", InboxConstants.ReplyTarget), ("data", data), ("template", "none")),
            cancellationToken);
    }

    // On the user's manager, with his manager and team as contexts: answers the whole inbox still needs.
    private async Task<int?> RequiredResponsesAsync(CancellationToken cancellationToken)
    {
        var record = await bridge.GetRecordAsync<BridgeRequiredResponsesDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props([InboxConstants.RequiredResponses]), cancellationToken);

        return record.Values.RequiredResponses;
    }

    // A message of the portal's whole list, by id, with the contexts its screen passes.
    private static BridgeQuery OnMessage(int messageId)
    {
        return BridgeQuery.FromHuman()
            .Base(string.Format(CultureInfo.InvariantCulture, InboxConstants.MessageById, messageId))
            .Context(InboxConstants.Tab, InboxConstants.AllMessagesTab)
            .Context(InboxConstants.SearchText, string.Empty)
            .WithHumanAndTeam();
    }
}
