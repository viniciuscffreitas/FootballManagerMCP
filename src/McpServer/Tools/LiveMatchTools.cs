using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class LiveMatchTools
{
    private const string Common =
        " By default a dry run that changes nothing: tell the user the change and why in one sentence, and only call " +
        "again with apply=true once they agreed. With apply=true the save is checked (and the real save copied) first, " +
        "and the answer carries the match read again as get_live_match answers it. Refused out of a match. Light: one " +
        "write and a few reads, safe while the match runs.";

    [McpServerTool(Name = "make_substitution")]
    [Description("Bring a substitute on for a player on the pitch in the user's match being played, as dragging him " +
                 "onto the player on the match screen does. Players by FM id from get_live_match (onPitch) and get_squad. " +
                 "The match's tactic swaps them at once (after shows who holds the place now) and the game makes the " +
                 "substitution at the next stoppage, about a minute of play later: only then does get_live_match list it " +
                 "and drop substitutionsRemaining. Refused when the player isn't on the pitch, the substitute isn't on " +
                 "the bench, or no substitutions are left." + Common)]
    public static async Task<string> MakeSubstitution(
        IHttpClientFactory httpFactory,
        [Description("FM id of the player on the pitch who goes off.")]
        int playerOffId,
        [Description("FM id of the substitute on the bench who comes on.")]
        int playerOnId,
        [Description("False (the default) only checks. True makes the substitution: only once the user has agreed.")]
        bool apply = false)
    {
        return await PostAsync(httpFactory, "substitution", apply, new { playerOffId, playerOnId });
    }

    [McpServerTool(Name = "set_live_mentality")]
    [Description("Change the mentality of the tactic the user's team is playing the match with, at once, as the " +
                 "match screen's mentality dropdown does. before and after are keys as the game reads them back; " +
                 "get_live_match's tactic.mentality shows it in the game's display language." + Common)]
    public static async Task<string> SetLiveMentality(
        IHttpClientFactory httpFactory,
        [Description("Mentality key: VeryDefensive, Defensive, Cautious, Balanced, Positive, Attacking or VeryAttacking.")]
        string mentality,
        [Description("False (the default) only checks. True changes it: only once the user has agreed.")]
        bool apply = false)
    {
        return await PostAsync(httpFactory, "mentality", apply, new { mentality });
    }

    [McpServerTool(Name = "set_live_role")]
    [Description("Change the role of one of the user's players on the pitch during the match, for the phase with the " +
                 "ball or without it, in the tactic the team plays (role keys from get_tactic_options). Refused when " +
                 "the team's tactic holds someone else at his place (possible after a substitution); nothing is written " +
                 "then. before and after are role keys as the game reads them back. Not yet confirmed in a live match " +
                 "that the match takes the new role at once." + Common)]
    public static async Task<string> SetLiveRole(
        IHttpClientFactory httpFactory,
        [Description("FM id of the player on the pitch.")]
        int playerId,
        [Description("Role key for the phase, e.g. InsideForward with the ball, or TrackingCentreForward without it.")]
        string role,
        [Description("True (the default) for his role with the ball, false for his role without it.")]
        bool inPossession = true,
        [Description("False (the default) only checks. True changes it: only once the user has agreed.")]
        bool apply = false)
    {
        return await PostAsync(httpFactory, "role", apply, new { playerId, inPossession, role });
    }

    [McpServerTool(Name = "give_team_talk")]
    [Description("The team talk to the whole squad on match day: before kick-off, at half-time and at full time, while " +
                 "the game shows its team talk panel (refused otherwise, e.g. while the ball rolls). Without apply it " +
                 "returns the panel's lines. With apply=true and line " +
                 "(its index from 0) it says that line, as clicking it does, and returns said, the squad's reaction " +
                 "(texts the panel showed after) and the lines left. Texts are in the game's display language. It " +
                 "doesn't press Continue: the user does, or the next team talk line is said in another call. The save " +
                 "is checked first.")]
    public static async Task<string> GiveTeamTalk(
        IHttpClientFactory httpFactory,
        [Description("The line to say, by its index on the panel from 0; left out, only reads the lines.")]
        int? line = null,
        [Description("False (the default) only reads. True says the line: only once the user has agreed.")]
        bool apply = false)
    {
        return await PostAsync(httpFactory, "team-talk", apply, new { line });
    }

    private static async Task<string> PostAsync(IHttpClientFactory httpFactory, string action, bool apply, object body)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/matches/live/{action}?apply={apply.ToString().ToLowerInvariant()}", body);
        return await response.ReadContentOrError();
    }
}
