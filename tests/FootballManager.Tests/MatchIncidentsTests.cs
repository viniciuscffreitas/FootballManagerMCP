using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Tests;

// Seen on 17/11/2026, Santos 3x1 Palmeiras: Neymar's penalty at 9', goals at 35' (with an assist), 45+1' and 73',
// yellow cards at 7', 11', 18', 48' and 71'.
public class MatchIncidentsTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(1, false)]
    [InlineData(3, false)]
    public void A_finished_match_counts_goals_and_penalty_goals(int type, bool isGoal)
    {
        Assert.Equal(isGoal, MatchIncidents.IsGoal(type, live: false));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(0, false)]
    [InlineData(38, false)]
    public void A_match_being_played_numbers_them_one_higher(int type, bool isGoal)
    {
        Assert.Equal(isGoal, MatchIncidents.IsGoal(type, live: true));
    }

    [Fact]
    public void An_incident_without_a_type_is_not_a_goal()
    {
        Assert.False(MatchIncidents.IsGoal(null, live: true));
        Assert.False(MatchIncidents.IsGoal(null, live: false));
    }
}
