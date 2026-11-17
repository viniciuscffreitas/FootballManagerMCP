using FootballManager.Domain.Conversations;

namespace FootballManager.Api.Requests.Conversations;

public class TalkToPlayerRequest
{
    /// <summary>The first-team player's FM id.</summary>
    public int PlayerId { get; set; }

    /// <summary>The conversation to open; not needed to continue the open one.</summary>
    public PlayerConversation? Conversation { get; set; }

    /// <summary>The line to say in each round, by its index in the round (from 0).</summary>
    public List<int> Choices { get; set; } = [];

    /// <summary>Says the choices on the card already open instead of opening one.</summary>
    public bool Continue { get; set; }
}
