using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Messages;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class NewsServiceTests
{
    private readonly INewsGameClient _game = Substitute.For<INewsGameClient>();
    private readonly NewsService _news;

    public NewsServiceTests()
    {
        _news = new NewsService(_game);
    }

    [Fact]
    public async Task The_team_and_league_news_come_back_together()
    {
        Feed(NewsFeed.Team, 66, Story(56448, "Oliva estreia pelo Santos Futebol Clube", "Planet Football"));
        Feed(NewsFeed.League, 97, Story(56449, "Balanta estreia pelo Mirassol Futebol Clube", "Expresso Esportivo Brazilian"));

        var news = (await _news.GetNewsAsync()).Value;

        Assert.Equal(66, news.Team.Total);
        Assert.Equal(("Oliva estreia pelo Santos Futebol Clube", "Planet Football"), (Assert.Single(news.Team.Stories).Title, news.Team.Stories[0].Source));
        Assert.Equal(97, news.League.Total);
        Assert.Equal(56449, Assert.Single(news.League.Stories).Id);
    }

    [Fact]
    public async Task Ten_stories_of_each_feed_without_their_text_by_default()
    {
        Feed(NewsFeed.Team, 0);
        Feed(NewsFeed.League, 0);

        await _news.GetNewsAsync();

        await _game.Received(1).GetNewsAsync(NewsFeed.Team, 10, false, Arg.Any<CancellationToken>());
        await _game.Received(1).GetNewsAsync(NewsFeed.League, 10, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_limit_and_the_text_apply_to_both_feeds()
    {
        Feed(NewsFeed.Team, 0);
        Feed(NewsFeed.League, 0);

        await _news.GetNewsAsync(limit: 3, includeBodies: true);

        await _game.Received(1).GetNewsAsync(NewsFeed.Team, 3, true, Arg.Any<CancellationToken>());
        await _game.Received(1).GetNewsAsync(NewsFeed.League, 3, true, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(NewsService.MaxLimit + 1)]
    public async Task A_limit_out_of_range_is_refused_without_reading_the_game(int limit)
    {
        var result = await _news.GetNewsAsync(limit);

        Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        Assert.Empty(_game.ReceivedCalls());
    }

    [Fact]
    public async Task No_game_is_a_game_unavailable_error()
    {
        _game.GetNewsAsync(Arg.Any<NewsFeed>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new GameUnavailableException("No save is loaded."));

        var result = await _news.GetNewsAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Feed(NewsFeed feed, int total, params NewsStory[] stories)
    {
        _game.GetNewsAsync(feed, Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new NewsPage { Total = total, Stories = [.. stories] });
    }

    private static NewsStory Story(int id, string title, string source)
    {
        return new NewsStory { Id = id, Date = "2026-03-15", Title = title, Subtitle = "Santos", Source = source };
    }
}
