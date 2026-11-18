using FootballManager.Domain.Scouting;

namespace FootballManager.Domain.Contracts;

/// <summary>
/// A change to one of the club's own players' contracts, made from his menu in the game: planned in a dry run, sent with
/// apply. Texts are in the game's display language.
/// </summary>
public class ContractChange
{
    public bool Applied { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }

    /// <summary>The menu action it goes through.</summary>
    public PlayerMenuAction Action { get; set; }

    /// <summary>The menu entry picked, as the game labels each level, e.g. "Definir status > Rotação".</summary>
    public required string Choice { get; set; }

    /// <summary>What the game's menu asks to confirm for the entry (for a release, what it costs), when it says.</summary>
    public string? Confirmation { get; set; }

    /// <summary>The game warns against it.</summary>
    public bool ConfirmationIsWarning { get; set; }

    /// <summary>The playing time his contract promises now (get_contracts' agreedPlayingTime), before the change.</summary>
    public string? AgreedPlayingTime { get; set; }

    /// <summary>Once applied: the playing time his contract promises after the change, as the game then shows it.</summary>
    public string? AgreedPlayingTimeAfter { get; set; }

    /// <summary>For a squad status: every status the menu offers him.</summary>
    public List<string> Choices { get; set; } = [];

    /// <summary>Once applied: the text of the confirmation dialog the game showed and the tool answered "Yes" to.</summary>
    public string? DialogAnswered { get; set; }

    /// <summary>Once applied: whether the first team still lists him (a release).</summary>
    public bool? StillInSquad { get; set; }

    public List<string> Notes { get; set; } = [];
}
