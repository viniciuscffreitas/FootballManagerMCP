namespace FootballManager.Domain.Transfers;

/// <summary>The game's transfer offer panel as it shows now (texts in the game's display language).</summary>
public class OfferPanel
{
    /// <summary>The player the offer is for, as the panel names him.</summary>
    public string? Player { get; set; }

    public string? Club { get; set; }

    /// <summary>The offer's fee as the panel's field holds it, in the game's currency units; null when unreadable.</summary>
    public long? Fee { get; set; }

    /// <summary>What the director of football says beside the offer: his starting point and suggested clauses.</summary>
    public List<string> Advice { get; set; } = [];

    /// <summary>Every text the panel shows, to tell what changed after a step.</summary>
    public List<string> Texts { get; set; } = [];
}
