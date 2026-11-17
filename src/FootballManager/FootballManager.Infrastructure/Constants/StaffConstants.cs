namespace FootballManager.Infrastructure.Constants;

// The first team's staff, as the game's staff screen reads it. Stars (CoachCurrentAbilityStarRange) come back null and
// responsibilities (ResponsibilityList) never answer through the plugin, so neither is read.
internal static class StaffConstants
{
    // Every staff member the game lists for the first team, the user's manager among them.
    public const string Members = TeamConstants.OwnTeam + ".AllStaffFilteredByTeam";

    public const string Job = "JobString";
    public const string CoachingStyle = "CoachingStyleString";
    public const string Wage = "FullContract.Wage.DisplayValue";
    public const string ContractEnd = "FullContract.EndDate";

    // Lists of the attributes the profile shows, each with its name, 1-20 value and the game's word for it. They need a
    // job type to answer; 0 reads them as the profile does.
    public const string CoachingAttributes = "AttributesCoaching";
    public const string MedicalAttributes = "AttributesMedical";
    public const string JobType = "JobType";
    public const int ProfileJobType = 0;

    public static readonly string[] MemberProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, GameConstants.Age, Job, CoachingStyle, Wage, ContractEnd,
        CoachingAttributes, MedicalAttributes
    ];

    // Counts on the first team; StaffAllCount is the club's whole staff.
    public const string AllCount = "StaffAllCount";
    public const string CoachingCount = "StaffCoachingCount";
    public const string MedicalCount = "StaffMedicalCount";
    public const string Analyst = "AnalystPerson.Name";

    public static readonly string[] TeamProperties = [GameConstants.Name, AllCount, CoachingCount, MedicalCount, Analyst];

    // Staff jobs the club has open, read on the club.
    public const string Club = "Club";
    public const string Vacancies = "StaffVacancies";
}
