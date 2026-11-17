namespace FootballManager.Api.Responses.Conversations;

public class TalkRoundResponse
{
    /// <summary>The lines the round offered, in the game's display language.</summary>
    public List<string> Lines { get; set; } = [];

    public int Chosen { get; set; }
    public required string ChosenLine { get; set; }

    /// <summary>What the card showed after the line that it didn't before: the player's answer.</summary>
    public List<string> Reaction { get; set; } = [];

    public string? BodyLanguage { get; set; }
}
