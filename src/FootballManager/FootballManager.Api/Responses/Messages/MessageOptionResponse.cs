namespace FootballManager.Api.Responses.Messages;

public class MessageOptionResponse
{
    /// <summary>What reply_to_message takes.</summary>
    public int Index { get; set; }

    public required string Title { get; set; }

    /// <summary>27 for a plain reply (EventFmxNewsResponseAction).</summary>
    public long Event { get; set; }

    public int? ResponseId { get; set; }
    public int? ResponseValue { get; set; }
    public int? NewsAction { get; set; }

    /// <summary>Only sent when true.</summary>
    public bool? IsDisabled { get; set; }
    public bool? IsRecommended { get; set; }

    public string? Confirmation { get; set; }

    /// <summary>What the option opens instead of answering; absent for a plain reply.</summary>
    public string? Flow { get; set; }
}
