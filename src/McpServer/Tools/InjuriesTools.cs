using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class InjuriesTools
{
    [McpServerTool(Name = "get_injuries")]
    [Description("The medical centre of the user's running Football Manager 27 save, read live from the game. Returns: " +
                 "players (every first-team player with FM id, injuriesThisSeason, riskLevel: the game's injury risk " +
                 "score, higher is riskier, riskOverview: the medical team's take on his risk, riskReason: what raises " +
                 "it, e.g. low condition, and history: every injury he has had, oldest first, with date, injury, " +
                 "duration as the game writes it and in days, severity Slight/Minor/Moderate/Major, cause, context " +
                 "(in training or in a match) and isActive for the one he has now); and season (the team's injuries " +
                 "this season, against last season, the staff's forecast for the season, the league average, counts " +
                 "by severity, how many players are at high risk, exhausted and recurring injuries, and who's expected " +
                 "back for the next match). For who can play the next match use get_availability. Texts are in the " +
                 "game's display language. This only reads. Not found when playerId isn't in the first team. Fails " +
                 "with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetInjuries(
        IHttpClientFactory httpFactory,
        [Description("Only this first-team player's record (FM id from get_squad); every player's by default, about 25 KB for a full squad.")]
        int? playerId = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var query = playerId is { } id ? $"?playerId={id}" : string.Empty;
        var response = await http.GetAsync($"/api/v1/injuries{query}");
        return await response.ReadContentOrError();
    }
}
