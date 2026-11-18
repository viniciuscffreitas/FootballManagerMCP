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

    [Fact]
    public async Task An_entry_under_a_submenu_the_game_greyed_out_is_refused_and_nothing_is_sent()
    {
        var plugin = new MenuPlugin(Course("Licença B", "Licença A", topDisabled: true));

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse, [0, 1], LicenceA));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task The_menu_is_checked_by_the_read_right_before_the_send()
    {
        var plugin = new MenuPlugin(Course("Licença B", "Licença A"));

        await Client(plugin).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse, [0, 1], LicenceA);

        var send = plugin.Requests.FindIndex(request => request.Method == HttpMethod.Post);
        Assert.Contains($"props={ScoutingConstants.MenuItems}", Uri.UnescapeDataString(plugin.Requests[send - 1].RequestUri!.Query));
    }

    [Fact]
    public async Task Actions_the_game_didnt_answer_are_unknown_not_none()
    {
        var plugin = new MenuPlugin(Course("Licença B", "Licença A"), actions: """{"values":{},"pending":["Actions"]}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => Client(plugin).GetOfferedActionsAsync(Coach));
    }

    private static StaffEditorBridgeClient Client(MenuPlugin plugin)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        return new StaffEditorBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
    }

    private static string Course(string first, string? second = null, bool lastDisabled = false, bool topDisabled = false)
    {
        var levels = new List<string> { Level(first, lastDisabled && second is null) };

        if (second is not null)
        {
            levels.Add(Level(second, lastDisabled));
        }

        return $$"""[{"Text":"Curso de treinador","Event":0,"Disabled":{{(topDisabled ? "true" : "false")}},"ChildList":[{{string.Join(',', levels)}}]}]""";
    }

    private static string Level(string label, bool disabled)
    {
        return $$$"""{"RowData":{"Text":"{{{label}}}","Event":44,"Disabled":{{{(disabled ? "true" : "false")}}}}}""";
    }

    // The plugin as the client sees it: the coaching course offered on the staff member, with the given entries.
    private sealed class MenuPlugin(string items, string? actions = null) : HttpMessageHandler
    {
        public List<Uri> Writes { get; } = [];
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (request.Method == HttpMethod.Post)
            {
                Writes.Add(request.RequestUri!);
                return Answer("{}");
            }

            var url = Uri.UnescapeDataString(request.RequestUri!.Query);

            return url.Contains($"props={ScoutingConstants.MenuItems}")
                ? Answer($$$"""{"values":{"Items":{{{items}}}}}""")
                : Answer(actions ?? $"{{\"values\":{{\"Actions\":{{\"#{StaffConstants.CoachingCourseAction}\":{{}}}}}}}}");
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
