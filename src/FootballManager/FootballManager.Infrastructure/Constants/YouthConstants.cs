namespace FootballManager.Infrastructure.Constants;

// The club's youth set-up and loans, as the game's development centre reads them, all from the club record.
internal static class YouthConstants
{
    public const string Club = "Club";

    // On the club: the loans summary, the youth team, the next intake and the youth ratings. YouthFacilityRating is a
    // record (rating out of 20 and its short and long texts); coaching is out of 20; recruitment is a small level (3).
    public const string LoanReport = "DevelopmentPlayersOnLoanReportText";
    public const string PlayersSuggestedForLoan = "PlayersSuggestedForLoanCount";
    public const string YouthTeamName = "HighestLevelYouthTeam.Name";
    public const string IntakeDate = "IntakeDate";
    public const string DaysUntilIntake = "DaysUntilYouthIntake";
    public const string FacilityRating = "YouthFacilityRating";
    public const string CoachingLevel = "YouthCoachingLevel";
    public const string Recruitment = "YouthRecruitment";
    public const string Candidates = "YouthCandidatesCount";

    public static readonly string[] SetupProperties =
    [
        LoanReport, PlayersSuggestedForLoan, YouthTeamName, IntakeDate, DaysUntilIntake, FacilityRating, CoachingLevel,
        Recruitment, Candidates
    ];

    // The club's players out on loan, each with the loan club's report on him. The fee comes as the game shows it.
    public const string Loans = "Club.PlayersOutOnLoan";
    public const string LoanClub = "LoanClub.Name";
    public const string LoanEndDate = "LoanContract.EndDate";
    public const string LoanMonthlyFee = "LoanContract.LoanPlayingMonthlyFee.DisplayValue";
    public const string LoanAppearances = "LoanAppearances";
    public const string LoanAverageRating = "LoanAverageRating";
    public const string LoanHappiness = "LoanHappiness";
    public const string LoanComments = "LoanReportComments";

    public static readonly string[] LoanProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, GameConstants.Age, LoanClub, LoanEndDate, LoanMonthlyFee,
        LoanAppearances, LoanAverageRating, LoanHappiness, LoanComments
    ];

    // The highest youth team's players, those out on loan included.
    public const string YouthPlayers = "Club.HighestLevelYouthTeam.Players";
    public const string ContractEndDate = "FullContract.EndDate";

    public static readonly string[] YouthPlayerProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, GameConstants.Age, GameConstants.Positions, GameConstants.Morale,
        ContractEndDate
    ];
}
