using System.Globalization;
using FootballManager.Domain.Staff;

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

    // A staff member as the record a menu action applies to: his item in the first team's staff list (seen 17/11/2026:
    // "Rescisão amigável", an EventFmxDoAction entry with the game's confirmation).
    public static string MemberRecord(int staffId)
    {
        return $"{Members}[{GameConstants.UniqueId}={staffId.ToString(CultureInfo.InvariantCulture)}]";
    }

    // Job adverts: the game hangs them on the first team (seen 17/11/2026: "Publicar Anúncio > Treino > Treinador", each
    // job an EventFmxDoAction entry); the club is tried after it. The club's own menu holds "Responsabilidades"
    // (TeamContainerEditResponsibilities), an entry that opens the responsibilities panel.
    public static readonly string[] ClubRecords = [TeamConstants.OwnTeam, Club];

    // FM.GamePlugin.ActionID values of the staff screen's menus.
    public const uint DismissAction = 1701671793;
    public const uint MutualTerminationAction = 1632134254;
    public const uint CoachingCourseAction = 1900504944;
    public const uint LeadershipCourseAction = 1297173097;
    // The same id as the player's "Iniciar negociação de contrato" (PersonOfferContract); not yet seen on a staff member.
    public const uint OfferContractAction = 2000111204;
    public const uint AdvertiseJobAction = 880036918;
    public const uint AdvertiseCoachingJobAction = 1951295084;
    public const uint AdvertiseScoutingJobAction = 1852196152;
    public const uint AdvertiseMedicalJobAction = 1816937269;

    public static uint ActionIdOf(StaffMenuAction action)
    {
        return action switch
        {
            StaffMenuAction.Dismiss => DismissAction,
            StaffMenuAction.MutualTermination => MutualTerminationAction,
            StaffMenuAction.CoachingCourse => CoachingCourseAction,
            StaffMenuAction.LeadershipCourse => LeadershipCourseAction,
            StaffMenuAction.OfferContract => OfferContractAction,
            StaffMenuAction.AdvertiseJob => AdvertiseJobAction,
            StaffMenuAction.AdvertiseCoachingJob => AdvertiseCoachingJobAction,
            StaffMenuAction.AdvertiseScoutingJob => AdvertiseScoutingJobAction,
            StaffMenuAction.AdvertiseMedicalJob => AdvertiseMedicalJobAction,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No menu action for it.")
        };
    }
}
