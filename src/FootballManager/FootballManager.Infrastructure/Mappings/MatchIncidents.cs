namespace FootballManager.Infrastructure.Mappings;

// Which of a match's incidents are goals. A finished match (the fixture) numbers a goal 0 and a penalty goal 2; the
// match being played (the Match root) numbers the same ones a step higher, 1 and 3, with a yellow card at 38. Seen on
// 17/11/2026 in Santos 3x1 Palmeiras, with Neymar's penalty at 9'.
internal static class MatchIncidents
{
    private const int Goal = 0;
    private const int PenaltyGoal = 2;
    private const int LiveGoal = 1;
    private const int LivePenaltyGoal = 3;

    public static bool IsGoal(int? type, bool live)
    {
        return live ? type is LiveGoal or LivePenaltyGoal : type is Goal or PenaltyGoal;
    }
}
