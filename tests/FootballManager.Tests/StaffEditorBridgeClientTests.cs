using System.Net;
using System.Text;
using FootballManager.Application.Exceptions;
using FootballManager.Domain.Staff;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

// The service picks an entry from the menu it read; the client reads the menu again to send it. The game can change the
// menu in between (the save guard copies the save first), so the client checks the entry is still the one picked.
public class StaffEditorBridgeClientTests
{
    private const int Coach = 7;
    private static readonly string[] LicenceA = ["Curso de treinador", "Licença A"];

    [Fact]
    public async Task The_entry_picked_is_sent_when_the_menu_still_has_it()
    {
        var plugin = new MenuPlugin(Course("Licença B", "Licença A"));

        await Client(plugin).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse, [0, 1], LicenceA);

        Assert.Single(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_that_changed_since_the_check_is_refused_and_nothing_is_sent()
    {
        var plugin = new MenuPlugin(Course("Licença B", "Licença Pro"));

        var refusal = await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse, [0, 1], LicenceA));

        Assert.Contains("Licença A", refusal.Message);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_the_game_greyed_out_since_the_check_is_refused_and_nothing_is_sent()
    {
        var plugin = new MenuPlugin(Course("Licença B", "Licença A", lastDisabled: true));

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse, [0, 1], LicenceA));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_the_menu_no_longer_reaches_is_refused_and_nothing_is_sent()
    {
        var plugin = new MenuPlugin(Course("Licença B"));

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse, [0, 1], LicenceA));

        Assert.Empty(plugin.Writes);
    }

    private static StaffEditorBridgeClient Client(MenuPlugin plugin)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        return new StaffEditorBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
    }

    private static string Course(string first, string? second = null, bool lastDisabled = false)
    {
        var levels = new List<string> { Level(first, lastDisabled && second is null) };

        if (second is not null)
        {
            levels.Add(Level(second, lastDisabled));
        }

        return $$"""[{"Text":"Curso de treinador","Event":0,"ChildList":[{{string.Join(',', levels)}}]}]""";
    }

    private static string Level(string label, bool disabled)
    {
        return $$$"""{"RowData":{"Text":"{{{label}}}","Event":44,"Disabled":{{{(disabled ? "true" : "false")}}}}}""";
    }

    // The plugin as the client sees it: the coaching course offered on the staff member, with the given entries.
    private sealed class MenuPlugin(string items) : HttpMessageHandler
    {
        public List<Uri> Writes { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post)
            {
                Writes.Add(request.RequestUri!);
                return Answer("{}");
            }

            var url = Uri.UnescapeDataString(request.RequestUri!.Query);

            return url.Contains($"props={ScoutingConstants.MenuItems}")
                ? Answer($$$"""{"values":{"Items":{{{items}}}}}""")
                : Answer($"{{\"values\":{{\"Actions\":{{\"#{StaffConstants.CoachingCourseAction}\":{{}}}}}}}}");
        }

        private static Task<HttpResponseMessage> Answer(string body)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
