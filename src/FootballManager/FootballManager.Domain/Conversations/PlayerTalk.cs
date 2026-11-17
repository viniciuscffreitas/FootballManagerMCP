namespace FootballManager.Domain.Conversations;

/// <summary>A conversation with a player, or what a dry run says it would do.</summary>
public class PlayerTalk
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }

    /// <summary>The conversation opened; null when an open one was continued.</summary>
    public PlayerConversation? Conversation { get; set; }

    /// <summary>False for a dry run, which neither opens nor says anything.</summary>
    public bool Applied { get; set; }

    /// <summary>The rounds played in this call, in order.</summary>
    public List<TalkRound> Rounds { get; set; } = [];

    /// <summary>
    /// The lines the open card offers now, when it waits for the user's choice: give it in a next call, with
    /// continue. Empty once the conversation has ended.
    /// </summary>
    public List<string> Lines { get; set; } = [];

    /// <summary>The player's body language as the card last showed it.</summary>
    public string? BodyLanguage { get; set; }

    /// <summary>Whether the card was closed at the end of this call.</summary>
    public bool Ended { get; set; }

    public List<string> Notes { get; set; } = [];
}
