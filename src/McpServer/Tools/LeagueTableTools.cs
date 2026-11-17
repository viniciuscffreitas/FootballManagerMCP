using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class LeagueTableTools
{
    [McpServerTool(Name = "get_league_table")]
    [Description("The user's league table in their running Football Manager 27 save, as the game's competition screen " +
                 "shows it, read live from the game. Returns: league (its name), teamName, position (the user's place, " +
                 "null when there's no table yet), mediaExpectedPosition (where the press expects the user's team to " +
                 "finish), and rows, top of the table first: position, FM team id, teamName, played, won, drawn, lost, " +
                 "goalsFor, goalsAgainst, goalDifference, points, movement (Same, Up or Down since the last result), " +
                 "fate (what the place leads to at the season's end, e.g. a promotion, when the game names one) and " +
                 "isUser (true on the user's line). When the league's season hasn't started, rows is empty and reason " +
                 "says why. Read only. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetLeagueTable(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/leaguetable");
        return await response.ReadContentOrError();
    }
}
