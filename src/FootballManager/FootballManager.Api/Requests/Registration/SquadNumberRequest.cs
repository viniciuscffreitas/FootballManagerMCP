namespace FootballManager.Api.Requests.Registration;

public class SquadNumberRequest
{
    public int? PlayerId { get; set; }
    public int? Number { get; set; }

    /// <summary>Ask the assistant to number everyone instead; then leave playerId and number out.</summary>
    public bool AskAssistant { get; set; }
}
