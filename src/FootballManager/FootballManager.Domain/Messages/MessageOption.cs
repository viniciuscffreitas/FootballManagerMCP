namespace FootballManager.Domain.Messages;

/// <summary>An answer the game offers to a message, as a button of the message (in its display language).</summary>
public class MessageOption
{
    /// <summary>Its place among the message's buttons, from 0: what reply_to_message takes.</summary>
    public int Index { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The native event the button sends: 27 (EventFmxNewsResponseAction) for a plain reply; 0 or another event for a
    /// button that opens a panel, a conversation or a tutorial.
    /// </summary>
    public long Event { get; set; }

    /// <summary>What a plain reply sends: the game's id and value for the answer.</summary>
    public int? ResponseId { get; set; }

    public int? ResponseValue { get; set; }

    /// <summary>The game's panel or flow the button opens (NewsAction), when it opens one.</summary>
    public int? NewsAction { get; set; }

    public bool IsDisabled { get; set; }

    /// <summary>The answer the game recommends.</summary>
    public bool IsRecommended { get; set; }

    /// <summary>The text of the dialog the game asks the user to confirm first, if any.</summary>
    public string? Confirmation { get; set; }

    /// <summary>For a button that opens a flow instead of replying: what it opens.</summary>
    public string? Flow { get; set; }
}
