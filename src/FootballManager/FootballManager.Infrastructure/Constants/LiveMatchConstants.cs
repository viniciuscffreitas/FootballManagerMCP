namespace FootballManager.Infrastructure.Constants;

// The match being played, as the match screen reads it. The game says whether a match is on (IsDuringMatch on the game
// record); while it is, the user's next managed fixture is that match (seen on 16/11/2026, Primavera x Santos), and the
// live-only properties (MatchTimeInMinutes, MatchHomeScore...) answer on it. Out of a match they stay pending.
internal static class LiveMatchConstants
{
    // On the game record.
    public const string IsDuringMatch = "IsDuringMatch";
    public const string CurrentDate = "CurrentGameDate";
    public static readonly string[] State = [IsDuringMatch, CurrentDate];

    // On the user's manager: the match, and which side is the user's.
    public const string Match = "NextManagedFixture";
    public const string MatchDate = Match + "." + MatchConstants.Date;
    public static readonly string[] Teams = [GameConstants.TeamId, MatchDate];

    // On the match. The fixture's own score and numbers are only written at the final whistle; the Match* ones follow
    // the play.
    public const string Minute = "MatchTimeInMinutes";
    public const string Clock = "MatchTime";
    public const string HomeScore = "MatchHomeScore";
    public const string AwayScore = "MatchAwayScore";

    public static readonly string[] Summary =
    [
        MatchConstants.Competition, MatchConstants.HomeTeamId, MatchConstants.HomeTeamName, MatchConstants.AwayTeamId,
        MatchConstants.AwayTeamName, Minute, Clock, HomeScore, AwayScore, MatchConstants.HomeScore, MatchConstants.AwayScore,
        MatchConstants.Incidents
    ];

    // On the tactic the team plays, which is the one the match uses. The substitution counts only answer in a match.
    public const string SubstitutionsRemaining = "SubsRemaining";
    public const string SubstitutionsAllowed = "TotalSubsAllowed";

    public static readonly string[] Tactic =
    [
        GameConstants.Name, TacticConstants.FormationName, TacticConstants.Mentality, SubstitutionsRemaining, SubstitutionsAllowed
    ];
}
