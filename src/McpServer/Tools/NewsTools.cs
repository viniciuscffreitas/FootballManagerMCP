using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class NewsTools
{
    [McpServerTool(Name = "get_news")]
    [Description("Get the newest news from the game's news portal, read live from the user's running Football Manager " +
                 "27 save: the stories about the user's club and the stories about his league, each newest first, " +
                 "which get_inbox doesn't carry (match reports, debuts, awards, rivals' results, managers' press " +
                 "conferences, bids and signings around the league). For each story: its id, date, title, subtitle " +
                 "(the club it's about) and source (the paper or site that published it); its text only with " +
                 "includeBodies. Also how many stories each feed holds. Text is in the game's display language, money " +
                 "as the game writes it. This only reads. Fails with a 503 when the game isn't running or no save is " +
                 "loaded.")]
    public static async Task<string> GetNews(
        IHttpClientFactory httpFactory,
        [Description("At most this many stories of the club's and of the league's news each (1-50), 10 by default. A story is about 150 bytes, more with its text.")]
        int? limit = null,
        [Description("Include each story's text (about 500 bytes to 1 KB each).")]
        bool includeBodies = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = (limit is { } max ? $"limit={max}&" : string.Empty) +
                    $"includeBodies={includeBodies.ToString().ToLowerInvariant()}";

        var response = await http.GetAsync($"/api/v1/news?{query}");
        return await response.ReadContentOrError();
    }
}
