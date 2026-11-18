using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class SetPiecesTools
{
    [McpServerTool(Name = "get_set_pieces")]
    [Description("Who wears the armband and who takes each set piece in the user's running Football Manager 27 save, " +
                 "read live from the game. Read only. Returns captain and viceCaptain (FM id and name), captainStandIns " +
                 "(who takes the armband when both are off the pitch, in order) and takerLists: one per list of the " +
                 "game's set piece screen (penalties, corners left and right, free kicks), each with its key, " +
                 "a label, and its takers in the order the game " +
                 "tries them. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetSetPieces(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/setpieces");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_captains")]
    [Description("Make a first-team player captain and/or another vice-captain, as the player's menu does in the game. " +
                 "By default a dry run that writes nothing: it checks the players and returns before (the setup now) " +
                 "and steps (what would change). A player who already holds the role is left as he is. With " +
                 "apply=true the API first checks and copies the save (see the README's Safety), writes, and returns " +
                 "after, read back from the game, with captainWritten and viceCaptainWritten (what the game took) and " +
                 "confirmed (true only when after shows every role asked for; read the notes when false). Refused when the two ids are the same player, when a player isn't in " +
                 "the first team, and when the game's menu doesn't offer the change.")]
    public static async Task<string> SetCaptains(
        IHttpClientFactory httpFactory,
        [Description("The player to make captain (FM id); leave out to keep the captain.")]
        int? captainId = null,
        [Description("The player to make vice-captain (FM id); leave out to keep the vice.")]
        int? viceCaptainId = null,
        [Description("False (the default) only checks. True writes: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/setpieces/captains?apply={apply.ToString().ToLowerInvariant()}", new { captainId, viceCaptainId });

        return await response.ReadContentOrError();
    }
}
