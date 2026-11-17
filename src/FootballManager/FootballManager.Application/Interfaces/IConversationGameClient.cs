using FootballManager.Domain.Conversations;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// One-to-one conversations with the user's players in the running game: the player's menu opens the conversation
/// card, whose lines are said one round at a time. Throws <see cref="Exceptions.GameUnavailableException"/> when the game
/// doesn't answer, and <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface IConversationGameClient
{
    /// <summary>The first-team player with that FM id and the conversations his menu offers now, or null.</summary>
    Task<ConversationPlayer?> GetPlayerAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>The conversation card open in the game, or null when none is.</summary>
    Task<ConversationCard?> ReadCardAsync(CancellationToken cancellationToken = default);

    /// <summary>Opens the conversation from the player's menu; the card once it shows, or null if it didn't.</summary>
    Task<ConversationCard?> OpenAsync(int playerId, PlayerConversation conversation, CancellationToken cancellationToken = default);

    /// <summary>Says the open card's line at that index; the card once it has answered, or null if it closed.</summary>
    Task<ConversationCard?> SayAsync(int line, CancellationToken cancellationToken = default);

    /// <summary>Ends the conversation and closes the card; whether it closed.</summary>
    Task<bool> EndAsync(CancellationToken cancellationToken = default);
}
