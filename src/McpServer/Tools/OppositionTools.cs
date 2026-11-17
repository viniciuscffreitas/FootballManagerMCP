using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class OppositionTools
{
    [McpServerTool(Name = "get_opposition_report")]
    [Description("The game's own opposition report on the user's next opponent, or on a team of the user's competitions, " +
                 "as the pre-match analysis in the user's running Football Manager 27 save shows it, read live. Returns: " +
                 "formation (the shape the game expects, as it names it, e.g. \"4-2-3-1 VOL MEA Com Pontas\"); " +
                 "tacticSummary (the game's line on how the team plays); expectedLineup (the eleven it expects, each with " +
                 "FM player id, name, position code like get_tactics' e.g. \"DCL\", in-possession roleKey like " +
                 "get_tactics' e.g. \"BallPlayingGoalkeeper\", and duty e.g. \"Support\"); points (the analysis' charts: " +
                 "topic, the game's verdict, the player it's about, the time frame); and, for the next opponent only, " +
                 "threatLevel (the game's assessment of the match). Complements get_team, which builds a report from " +
                 "the team's match numbers. Texts are in the game's display language. Read only. Fails with a 404 when " +
                 "no team is named and the user has no match ahead, a 400 for an unknown team, and a 503 when the game " +
                 "isn't running.")]
    public static async Task<string> GetOppositionReport(
        IHttpClientFactory httpFactory,
        [Description("Optional. A team of the user's league or competitions, by its name as the game writes it (case " +
                     "and accents don't matter) or its FM id. Left out, the user's next opponent.")]
        string? team = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var query = string.IsNullOrWhiteSpace(team) ? string.Empty : $"?team={Uri.EscapeDataString(team)}";
        var response = await http.GetAsync($"/api/v1/opposition{query}");
        return await response.ReadContentOrError();
    }
}
