namespace FootballManager.Api.Responses.Messages;

public class MessageOptionsResponse
{
    public int MessageId { get; set; }
    public required string Title { get; set; }
    public bool CanReply { get; set; }
    public bool HasReplied { get; set; }

    /// <summary>The message needs an answer before Continue moves on.</summary>
    public bool HasRequiredAction { get; set; }

    /// <summary>Answers the whole inbox still needs before Continue moves on.</summary>
    public int? RequiredResponses { get; set; }

    public List<MessageOptionResponse> Options { get; set; } = [];
}
