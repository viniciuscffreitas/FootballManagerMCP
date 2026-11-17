namespace FootballManager.Infrastructure.Constants;

// The match being played, as the match screen reads it. The game says whether a match is on (IsDuringMatch on the game
// record); while it is, the user's next managed fixture is that match (seen on 16/11/2026, Primavera x Santos). The
// live properties (MatchTime, MatchHomeScore, Stats, PlayerMatchStats...) answer on the "Match" root only: on the
// fixture they stay pending or empty during the match (seen on 17/11/2026, Atlético Mineiro x Santos).
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
    // the play. MatchTimeInMinutes answers null; the clock ("18:45", minutes and seconds) gives the minute.
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

    // On the tactic the team plays, which is the one the match uses.
    public static readonly string[] Tactic = [GameConstants.Name, TacticConstants.FormationName, TacticConstants.Mentality];

    // On one side of the match (context isHomeTeam): its numbers and its match tactic's substitutions. The counts stay
    // pending on the team's tactic; they answer on the match's.
    public const string SubstitutionsRemaining = "Tactic.SubsRemaining";
    public const string SubstitutionsAllowed = "Tactic.TotalSubsAllowed";
    public static readonly string[] Side = [MatchConstants.TeamStats, SubstitutionsRemaining, SubstitutionsAllowed];

    // Each player who took the pitch for a side (a list on the match, context isHomeTeam). On the match the list holds
    // references, so its items are read field by field.
    public static readonly string[] PlayerLine =
    [
        "Player", "IncidentType", "MatchTime", "PlayerTeamSelectionIndex", "YellowCardedInMatch", "RedCardedInMatch",
        "TotalRating", "Goals"
    ];
}
