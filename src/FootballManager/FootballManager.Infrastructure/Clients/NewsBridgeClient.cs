using FootballManager.Application.Interfaces;
using FootballManager.Domain.Messages;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Messages;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the portal's club and league news through the plugin, newest first, as the game lists them. Only reads.
internal sealed class NewsBridgeClient(GameBridge bridge) : INewsGameClient
{
    public async Task<NewsPage> GetNewsAsync(NewsFeed feed, int take, bool includeBodies, CancellationToken cancellationToken = default)
    {
        var list = feed == NewsFeed.Team
            ? BridgeQuery.FromHuman().List(NewsConstants.TeamNews).Ref(NewsConstants.TeamContext, NewsConstants.Team)
            : BridgeQuery.FromHuman().List(NewsConstants.LeagueNews).Ref(NewsConstants.LeagueContext, NewsConstants.League);

        var paths = includeBodies ? [.. NewsConstants.StoryProperties, InboxConstants.Body] : NewsConstants.StoryProperties;

        var page = await bridge.GetListAsync<BridgeNewsStoryDto>(
            list.Context(NewsConstants.Number, NewsConstants.AnyNumber).WithHumanAndTeam().Take(take).Props(paths),
            cancellationToken, gapsExpected: true);

        return new NewsPage
        {
            Total = page.Total ?? page.Count,
            Stories = page.Items.Select(story => story.Values).ToList().Adapt<List<NewsStory>>()
        };
    }
}
