namespace FootballManager.Domain.Scouting;

/// <summary>
/// The text of a confirmation the game asks before a menu entry runs (for a release, its cost), compared case and runs
/// of white space aside.
/// </summary>
public static class ConfirmationText
{
    /// <summary>Lower case, every run of white space as one space, trimmed.</summary>
    public static string Normalize(string? text)
    {
        return string.Join(' ', (text ?? string.Empty).ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>Both say the same, case and white space aside.</summary>
    public static bool SameAs(string? text, string? other)
    {
        return Normalize(text) == Normalize(other);
    }

    /// <summary>
    /// The dialog's text is the confirmation or holds the whole of it. An empty confirmation is held by no dialog: there
    /// is nothing to tell the action's dialog from another one by.
    /// </summary>
    public static bool IsHeldBy(string? dialogText, string? confirmation)
    {
        var expected = Normalize(confirmation);
        return expected.Length > 0 && Normalize(dialogText).Contains(expected, StringComparison.Ordinal);
    }
}
