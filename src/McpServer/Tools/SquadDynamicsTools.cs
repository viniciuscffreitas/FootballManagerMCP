using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class SquadDynamicsTools
{
    [McpServerTool(Name = "get_squad_dynamics")]
    [Description("The dressing room of the user's team in their running Football Manager 27 save, read live from the " +
                 "game's squad dynamics screen. Returns: hierarchy (the players on each level, from teamLeaders to " +
                 "highlyInfluential, influential and other, with FM id and name); atmosphere (the game's verdict, " +
                 "e.g. \"Muito bom\", its 0-100 score, and the reasons it gives, each with the game's weight: 2 seen " +
                 "on positive reasons, 0 on a negative one); issues (open happiness issues: the player who raised " +
                 "it, severity, description, how many teammates agree, and whether a promise is tied to it); " +
                 "promises (the promises the manager made to players: title, description, status, days left, due " +
                 "date); and players (every first-team player with FM id, hierarchy level, and the game's reasons " +
                 "he is happy and unhappy, plus how many promises he holds). Texts are in the game's display " +
                 "language. Read only. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetSquadDynamics(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/squaddynamics");
        return await response.ReadContentOrError();
    }
}
