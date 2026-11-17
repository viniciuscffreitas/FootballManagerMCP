namespace FootballManager.Domain.Conversations;

/// <summary>A first-team player and the conversations his menu offers now.</summary>
public class ConversationPlayer
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// The conversations the game offers for him now. One leaves the list once used, even when its card was closed
    /// without an answer, and some depend on where he stands (an injured player can't be praised for his performances).
    /// </summary>
    public List<PlayerConversation> Available { get; set; } = [];
}
