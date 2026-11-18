namespace FootballManager.Domain.Matches;

/// <summary>
/// A place in the user's matchday squad as the match's tactic holds it: 0-10 on the pitch, 11 on the bench, with the
/// player there and the place's position (a position mask; 0 on the bench).
/// </summary>
public class MatchPlace
{
    public int Index { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public ulong PositionMask { get; set; }

    public const int FirstBenchIndex = 11;

    public bool OnPitch => Index < FirstBenchIndex;
}
