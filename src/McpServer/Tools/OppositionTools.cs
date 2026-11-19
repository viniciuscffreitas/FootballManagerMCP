using System.ComponentModel;
using System.Net.Http.Json;
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

    [McpServerTool(Name = "set_opposition_instructions")]
    [Description("Read and change the instructions against the user's next opponent, as the opposition instructions " +
                 "screen of the user's running Football Manager 27 save gives them, on a player of the lineup the game " +
                 "expects from the opponent. Each player takes four: marking (tight marking: none, never, always), " +
                 "pressing (press him when he gets the ball: none, never, always), tackling (none, normal, soft, hard) " +
                 "and showOntoFoot (none, left, right, weaker). action is required: Set (give the player the " +
                 "instructions named; the others stay), Clear (take every instruction off the player), ApplyStaffAdvice " +
                 "(give the instructions the staff suggests, to every player or only to the one named). By default a dry " +
                 "run: returns current (players: each lineup player with FM id, name, position code like \"AMC\" and " +
                 "his four instructions; positions: the same per position, read only; staffSuggestions: the staff's " +
                 "lines in the game's language, each with the instruction and value it maps to, or null when its kind " +
                 "isn't mapped yet and so isn't applied) and plan (each write: player, instruction, from, to). Show the " +
                 "user the plan and ask. To apply, call again with apply=true: the save is checked (and the real one " +
                 "copied) first; after holds the instructions read back and plan[].confirmed says whether each write " +
                 "shows. Fails with a 400 for a player outside the expected lineup or a value the instruction doesn't " +
                 "take, a 404 when the user has no match ahead, and a 503 when the game isn't running. Don't run it " +
                 "while a match is being played.")]
    public static async Task<string> SetOppositionInstructions(
        IHttpClientFactory httpFactory,
        [Description("Set, Clear or ApplyStaffAdvice.")]
        string action,
        [Description("An opponent player of the expected lineup, by FM id or name (case and accents don't matter). " +
                     "Needed for Set and Clear; for ApplyStaffAdvice, only that player's suggestions.")]
        string? player = null,
        [Description("Tight marking: none, never or always.")]
        string? marking = null,
        [Description("Pressing him when he gets the ball: none, never or always.")]
        string? pressing = null,
        [Description("Tackling: none, normal, soft or hard.")]
        string? tackling = null,
        [Description("The foot to show him onto: none, left, right or weaker.")]
        string? showOntoFoot = null,
        [Description("False (the default) only plans. True writes: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/opposition/instructions?apply={apply.ToString().ToLowerInvariant()}",
            new { action, player, marking, pressing, tackling, showOntoFoot });

        return await response.ReadContentOrError();
    }
}
