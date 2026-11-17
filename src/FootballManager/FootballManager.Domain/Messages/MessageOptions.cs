namespace FootballManager.Domain.Messages;

/// <summary>The answers the game offers to a message, read without opening it.</summary>
public class MessageOptions
{
    public int MessageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool CanReply { get; set; }
    public bool HasReplied { get; set; }

    /// <summary>The message needs an answer before Continue moves the calendar on.</summary>
    public bool HasRequiredAction { get; set; }

    public List<MessageOption> Options { get; set; } = [];

    /// <summary>
    /// How many answers the inbox still needs before Continue moves the calendar on (ManagerResponseRequiredCount), across
    /// all messages.
    /// </summary>
    public int? RequiredResponses { get; set; }
}
