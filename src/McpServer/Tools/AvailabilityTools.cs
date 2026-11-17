using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class AvailabilityTools
{
    [McpServerTool(Name = "get_availability")]
    [Description("Who can play the user's next match in their running Football Manager 27 save, read live from the game. " +
                 "Returns: nextMatch (the competition, its registration/eligibility rules, empty when it has none, and " +
                 "the squad size rule with the squad's count against it, e.g. \"50/50\"); injured (players an injury " +
                 "keeps out: the injury, the game's expected return and days until it); suspended (every ban the " +
                 "team's players serve: the scope it applies to, e.g. the national league, matches or days left, and " +
                 "the reason, e.g. 3 yellow cards); and players (every first-team player with FM id, positions, best " +
                 "role, condition %, match sharpness %, average rating this season, available, and absences: " +
                 "Injured and/or Suspended). A ban only rules a player out of the competitions its scope covers, so " +
                 "compare it with nextMatch.competition. Texts are in the game's display language. Fails with a 503 " +
                 "when the game isn't running or no save is loaded.")]
    public static async Task<string> GetAvailability(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/availability");
        return await response.ReadContentOrError();
    }
}
