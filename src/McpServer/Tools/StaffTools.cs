using System.ComponentModel;
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
}
