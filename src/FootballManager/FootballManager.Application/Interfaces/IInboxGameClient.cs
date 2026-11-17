using FootballManager.Domain.Messages;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the user's inbox in the running game and answers its messages. Throws <see cref="Exceptions.GameUnavailableException"/> when the game
/// can't be reached.
/// </summary>
public interface IInboxGameClient
{
    /// <summary>
    /// The messages from <paramref name="skip"/> on, at most <paramref name="take"/>: the whole inbox, or only the
    /// unread messages. A forwarded article's text comes only with <paramref name="includeArticles"/>.
    /// </summary>
    Task<Inbox> GetInboxAsync(bool unreadOnly, int skip, int take, bool includeArticles, CancellationToken cancellationToken = default);

    /// <summary>
    /// The answers the game offers to the message (its buttons), read from the inbox without opening it, with how many
    /// answers the inbox still needs. Null when the inbox holds no message with that id.
    /// </summary>
    Task<MessageOptions?> GetMessageOptionsAsync(int messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a plain answer to the message, as its button does: the game's EventFmxNewsResponseAction with the
    /// option's id and value. Throws <see cref="Exceptions.GameRejectedException"/> when the game refuses it.
    /// </summary>
    Task SendReplyAsync(int messageId, int responseId, int? responseValue, CancellationToken cancellationToken = default);
}
