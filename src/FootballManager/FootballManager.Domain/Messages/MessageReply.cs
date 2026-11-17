namespace FootballManager.Domain.Messages;

/// <summary>An answer to a message: planned in a dry run, sent with apply.</summary>
public class MessageReply
{
    public bool Applied { get; set; }
    public int MessageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public required MessageOption Option { get; set; }

    /// <summary>Read back after sending: the game took the answer.</summary>
    public bool? HasReplied { get; set; }

    /// <summary>Answers the inbox still needs before Continue moves on: before a dry run, after an apply.</summary>
    public int? RequiredResponses { get; set; }

    public List<string> Notes { get; set; } = [];
}
