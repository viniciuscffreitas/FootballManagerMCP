using FootballManager.Domain.Conversations;

namespace FootballManager.Api.Responses.Conversations;

public class ConversationPlayerResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>The conversations his menu offers now.</summary>
    public List<PlayerConversation> Available { get; set; } = [];
}
