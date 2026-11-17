namespace FootballManager.Api.Responses.Messages;

public class MessageReplyResponse
{
    public bool Applied { get; set; }
    public int MessageId { get; set; }
    public required string Title { get; set; }
    public required MessageOptionResponse Option { get; set; }

    /// <summary>Read back after an apply.</summary>
    public bool? HasReplied { get; set; }

    public int? RequiredResponses { get; set; }
    public List<string> Notes { get; set; } = [];
}
