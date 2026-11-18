using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class StaffTools
{
    [McpServerTool(Name = "get_staff")]
    [Description("The first team's staff in the user's running Football Manager 27 save, read live from the game. " +
                 "Returns: members (every staff member the game lists for the first team, the user's manager among " +
                 "them: FM id, name, age, job, coaching style, coachingAttributes and medicalAttributes as name, value " +
                 "1-20 and the game's word for it, the weekly wage as the game writes it, e.g. \"R$13 mil p/s\", or " +
                 "\"N/A\" without a contract, and contractEnd yyyy-MM-dd); allCount (the club's whole staff), " +
                 "coachingCount, medicalCount, analyst (the analyst the game names for the team, not necessarily the " +
                 "performance analyst) and vacancies (staff jobs the club has open). Staff stars and responsibilities " +
                 "aren't read. Texts are in the game's display language. Fails with a 503 when the game isn't running " +
                 "or no save is loaded.")]
    public static async Task<string> GetStaff(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/staff");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "manage_staff")]
    [Description("Run a staff action through the game's own menu, as the staff screen offers it. On a first-team " +
                 "staff member (staffId from get_staff): Dismiss (sack, paying off his contract), MutualTermination " +
                 "(end the contract by agreement), CoachingCourse, LeadershipCourse, OfferContract (opens the game's " +
                 "contract offer to him). On the club (leave staffId out): AdvertiseJob, AdvertiseCoachingJob, " +
                 "AdvertiseScoutingJob, AdvertiseMedicalJob (place a job advert). The game offers only what fits now " +
                 "(no sacking the user's own manager, no course for someone already on one): an action it doesn't " +
                 "offer fails with the ones it does. By default a dry run that returns the menu's entries in the " +
                 "game's language, the entry it would pick and the game's confirmation (e.g. the compensation a " +
                 "sacking costs). When the menu has several entries (a course's levels, an advert's jobs), name one by " +
                 "its label at each level in entry, e.g. [\"Curso de treinador\", \"Licença A\"]. Show the user the " +
                 "plan and ask. To apply, call again with apply=true: the save is checked (and the real one copied) " +
                 "first; stillOffered then says whether the menu still offers the action. Refused while a match is " +
                 "being played.")]
    public static async Task<string> ManageStaff(
        IHttpClientFactory httpFactory,
        [Description("Dismiss, MutualTermination, CoachingCourse, LeadershipCourse, OfferContract, AdvertiseJob, " +
                     "AdvertiseCoachingJob, AdvertiseScoutingJob or AdvertiseMedicalJob.")]
        string action,
        [Description("The staff member's FM id from get_staff; leave out for a job advert.")]
        int? staffId = null,
        [Description("The menu entry to pick, by its label at each level as the dry run lists them.")]
        List<string>? entry = null,
        [Description("False (the default) only checks. True runs it: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/staff/actions?apply={apply.ToString().ToLowerInvariant()}",
            new { staffId, action, entry });

        return await response.ReadContentOrError();
    }
}
