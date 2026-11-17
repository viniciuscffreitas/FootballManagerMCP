namespace FootballManager.Domain.Conversations;

/// <summary>A round of a conversation: the lines the card offered, the one said, and how the player took it.</summary>
public class TalkRound
{
    public List<string> Lines { get; set; } = [];
    public int Chosen { get; set; }
    public required string ChosenLine { get; set; }

    /// <summary>What the card showed after the line that it didn't show before: the player's answer.</summary>
    public List<string> Reaction { get; set; } = [];

    public string? BodyLanguage { get; set; }
}
