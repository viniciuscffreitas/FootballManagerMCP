using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class RotationTools
{
    [McpServerTool(Name = "suggest_rotation")]
    [Description("Rotate the starting eleven of the active tactic for the user's next match in the running game, " +
                 "read live. A starter goes out when he is injured, banned (a ban counts when its scope names the " +
                 "next match's competition; one whose scope can't be matched to it is treated as out, and the reason " +
                 "says so) or tired (condition under 85). A reserve comes in when he is available, has condition 92 " +
                 "or more and is rated in the slot's role at its position (get_role_fit's rating, 0-20): one with " +
                 "match sharpness 75+ first, then the best rated. A tired starter only goes out for a reserve rated " +
                 "at most 2 below him; an injured or banned one for anyone who qualifies. Each slot keeps its " +
                 "position and role. Returns swaps (slot index, position, role, who goes out and why, who comes in " +
                 "with rating, condition and sharpness, and a detail when he isn't match-sharp), unfilled (starters " +
                 "who should go out but no reserve qualifies for: they stay, pick someone with set_tactic), the next " +
                 "match's rules, notes (registration rules the game can't check per player: tell the user to confirm " +
                 "those coming in are registered) and edit, set_tactic's steps for the picks. By default a dry run " +
                 "that changes nothing: show the user each swap with its reason and ask. With apply=true the picks " +
                 "go through set_tactic (save checked and copied first) and each step comes back Done or Failed. " +
                 "Running it again right after changes nobody. The bench isn't rebuilt: a reserve from the bench " +
                 "swaps places with the starter, one from outside the matchday squad takes the slot and the starter " +
                 "leaves the squad, so check the bench with set_tactic afterwards. Refused while a match is being played.")]
    public static async Task<string> SuggestRotation(
        IHttpClientFactory httpFactory,
        [Description("False (the default) only plans. True picks the reserves in the game: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsync($"/api/v1/rotation?apply={apply.ToString().ToLowerInvariant()}", content: null);
        return await response.ReadContentOrError();
    }
}
