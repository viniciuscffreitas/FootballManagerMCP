using FootballManager.Domain.Scouting;

namespace FootballManager.Domain.Staff;

/// <summary>A staff menu action as planned or sent: who it's on, the menu the game offers for it and the entry picked.</summary>
public class StaffAction
{
    public bool Applied { get; set; }
    public StaffMenuAction Action { get; set; }

    /// <summary>The staff member it's on; null for the club's job adverts.</summary>
    public int? StaffId { get; set; }

    public string? StaffName { get; set; }

    /// <summary>The menu's entries as the game offers them now, in its display language.</summary>
    public List<MenuEntry> Menu { get; set; } = [];

    /// <summary>The entry picked, each level's label joined by " > "; null when the menu has several and none was named.</summary>
    public string? Choice { get; set; }

    /// <summary>What the game asks the user to confirm for that entry, when it asks.</summary>
    public string? Confirmation { get; set; }

    public bool ConfirmationIsWarning { get; set; }

    /// <summary>After an apply: whether the game's menu still offers the action (a course or a sacking takes it away).</summary>
    public bool? StillOffered { get; set; }

    public List<string> Notes { get; set; } = [];
}
