namespace FootballManager.Domain.Contracts;

/// <summary>How the club lets one of its own players go, as the entries of his menu do.</summary>
public enum ReleaseKind
{
    /// <summary>Released on a free transfer at once: the club pays what's left of his contract. Can't be undone.</summary>
    Free,

    /// <summary>His contract ended by mutual consent: offered to him, he accepts or refuses.</summary>
    Mutual,

    /// <summary>Marked to leave when his contract ends: he stays until then.</summary>
    EndOfContract
}
