using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

public interface IInboxService
{
    /// <summary>A page of the user's inbox, newest first.</summary>
    Task<Result<Inbox>> GetInboxAsync(
        bool unreadOnly = false, int? limit = null, int skip = 0, bool includeArticles = false, CancellationToken cancellationToken = default);

    /// <summary>The answers the game offers to a message, without opening it, and how many answers the inbox still needs.</summary>
    Task<Result<MessageOptions>> GetMessageOptionsAsync(int messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Answers a message with one of its options (by index), as its button would. Only a plain reply is sent; an option
    /// that opens a flow is refused with what it opens. Writes only when <paramref name="apply"/> is set, through the
    /// save guard.
    /// </summary>
    Task<Result<MessageReply>> ReplyAsync(int messageId, int option, bool apply, CancellationToken cancellationToken = default);
}

public class InboxService(IInboxGameClient gameClient, ISaveGuard saveGuard) : IInboxService
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    /// <summary>The native event of a plain reply button, EventFmxNewsResponseAction.</summary>
    public const long ReplyEvent = 27;

    // The message shows the answer a frame or more after it's sent.
    private const int StateChecks = 5;
    private static readonly TimeSpan StatePoll = TimeSpan.FromMilliseconds(300);

    // "Começar Introdução" on the assistant's introductions: the tutorial, whatever its NewsAction.
    private const long TutorialEvent = 1231635054;

    // The panels a message's button opens, by NewsAction, as seen in the game (transfer and contract by
    // OpenNewsActionPanelEvent; the others on their messages).
    private static readonly Dictionary<int, string> Flows = new()
    {
        [6] = "the transfer offer panel",
        [7] = "the contract negotiation panel",
        [10] = "the competition registration panel",
        [14] = "a conversation with the player",
        [33] = "the squad meeting",
        [108] = "negotiate the board's vision",
        [173] = "the club's vision screen"
    };

    public async Task<Result<Inbox>> GetInboxAsync(
        bool unreadOnly = false, int? limit = null, int skip = 0, bool includeArticles = false, CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > MaxLimit)
        {
            return Result.Fail<Inbox>(new ValidationError("limit", [$"The limit runs 1 to {MaxLimit}."]));
        }

        if (skip < 0)
        {
            return Result.Fail<Inbox>(new ValidationError("skip", ["Skip can't be negative."]));
        }

        try
        {
            return Result.Ok(await gameClient.GetInboxAsync(unreadOnly, skip, limit ?? DefaultLimit, includeArticles, cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<Inbox>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<MessageOptions>> GetMessageOptionsAsync(int messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await gameClient.GetMessageOptionsAsync(messageId, cancellationToken) is not { } message)
            {
                return Result.Fail<MessageOptions>(new EntityDoesNotExistError());
            }

            foreach (var option in message.Options)
            {
                option.Flow = FlowOf(option);
            }

            return Result.Ok(message);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<MessageOptions>(new GameUnavailableError(exception.Message));
        }
    }

    private static Result<MessageReply> Refuse(string reason)
    {
        return Result.Fail<MessageReply>(new ValidationError("option", [reason]));
    }

    // A plain reply sends EventFmxNewsResponseAction and opens nothing. Any other button opens a panel (event 0 with
    // the panel's NewsAction), a conversation or the tutorial: what it opens, by the ids seen in the game.
    private static string? FlowOf(MessageOption option)
    {
        if (option.Event == ReplyEvent && option.NewsAction is null)
        {
            return null;
        }

        if (option.Event == TutorialEvent)
        {
            return "the assistant's tutorial introduction";
        }

        return option.NewsAction is { } panel && Flows.TryGetValue(panel, out var flow)
            ? flow
            : option.NewsAction is { } other
                ? $"the game's panel NewsAction {other}"
                : $"the game's event {option.Event}";
    }

    public async Task<Result<MessageReply>> ReplyAsync(int messageId, int option, bool apply, CancellationToken cancellationToken = default)
    {
        var read = await GetMessageOptionsAsync(messageId, cancellationToken);

        if (read.IsFailed)
        {
            return Result.Fail<MessageReply>(read.Errors);
        }

        var message = read.Value;

        if (message.HasReplied)
        {
            return Refuse($"Message {messageId} is already answered.");
        }

        if (option < 0 || option >= message.Options.Count)
        {
            return Refuse(message.Options.Count == 0
                ? $"Message {messageId} offers no answer."
                : $"Message {messageId} has no option {option}. Its options: " +
                  string.Join(", ", message.Options.Select(o => $"{o.Index} {o.Title}")) + ".");
        }

        var picked = message.Options[option];

        if (picked.Flow is { } flow)
        {
            return Refuse($"'{picked.Title}' doesn't answer the message: it opens {flow}, which this tool doesn't drive. " +
                          "Pick a plain reply, or open it in the game.");
        }

        if (picked.IsDisabled)
        {
            return Refuse($"The game shows '{picked.Title}' disabled: it can't be picked now.");
        }

        if (picked.ResponseId is null)
        {
            return Refuse($"The game gives '{picked.Title}' no answer id to send.");
        }

        var reply = new MessageReply
        {
            Applied = false,
            MessageId = message.MessageId,
            Title = message.Title,
            Option = picked,
            RequiredResponses = message.RequiredResponses
        };

        if (!apply)
        {
            return Result.Ok(reply);
        }

        var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

        if (clearance.IsFailed)
        {
            return Result.Fail<MessageReply>(clearance.Errors);
        }

        reply.Notes.Add(SaveNote(clearance.Value));

        try
        {
            await gameClient.SendReplyAsync(messageId, picked.ResponseId.Value, picked.ResponseValue, cancellationToken);
            reply.Applied = true;
            await ReadBackAsync(reply, cancellationToken);
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<MessageReply>(new ValidationError("game", [exception.Message]));
        }
        catch (GameUnavailableException exception) when (!reply.Applied)
        {
            return Result.Fail<MessageReply>(new GameUnavailableError(exception.Message));
        }
        catch (GameUnavailableException exception)
        {
            reply.Notes.Add($"The answer was sent, but the game stopped answering before it could be read back ({exception.Message}).");
        }

        return Result.Ok(reply);
    }

    private async Task ReadBackAsync(MessageReply reply, CancellationToken cancellationToken)
    {
        for (var check = 0; check < StateChecks; check++)
        {
            if (check > 0)
            {
                await Task.Delay(StatePoll, cancellationToken);
            }

            if (await gameClient.GetMessageOptionsAsync(reply.MessageId, cancellationToken) is not { } after)
            {
                reply.Notes.Add("The message is no longer in the inbox's list, so the answer couldn't be read back.");
                return;
            }

            reply.HasReplied = after.HasReplied;
            reply.RequiredResponses = after.RequiredResponses;

            if (after.HasReplied)
            {
                return;
            }
        }

        reply.Notes.Add("The game doesn't show the message as answered yet: check get_message_options before sending it again.");
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
