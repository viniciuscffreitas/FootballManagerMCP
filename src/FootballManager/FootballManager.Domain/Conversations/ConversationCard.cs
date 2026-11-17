namespace FootballManager.Domain.Conversations;

/// <summary>The open conversation card, as the game shows it, in its display language.</summary>
public class ConversationCard
{
    /// <summary>The lines the user can say now, in the card's order; empty once the conversation has no more rounds.</summary>
    public List<string> Lines { get; set; } = [];

    /// <summary>The player's body language, as the card describes it.</summary>
    public string? BodyLanguage { get; set; }

    /// <summary>The card's other texts, in order (the player's name, his answers), without the lines and the body language.</summary>
    public List<string> Texts { get; set; } = [];
}
