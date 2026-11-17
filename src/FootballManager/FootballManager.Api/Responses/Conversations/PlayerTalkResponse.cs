using FootballManager.Domain.Conversations;

namespace FootballManager.Api.Responses.Conversations;

public class PlayerTalkResponse
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerConversation? Conversation { get; set; }
    public bool Applied { get; set; }
    public List<TalkRoundResponse>? Rounds { get; set; }

    /// <summary>The lines the open card waits on: choose one in a next call, with continue.</summary>
    public List<string>? Lines { get; set; }

    public string? BodyLanguage { get; set; }
    public bool Ended { get; set; }
    public List<string>? Notes { get; set; }
}
