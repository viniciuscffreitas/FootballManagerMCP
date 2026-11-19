using System.Net;
using System.Text;
using FootballManager.Application.Exceptions;
using FootballManager.Domain.Opposition;
using FootballManager.Infrastructure.Clients;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

// The screen's rows hold players by record; the opponent's squad names them. A write goes to the row found again right
// before it is sent, so a lineup the game changed in between never takes another player's instruction.
public class OppositionInstructionsBridgeClientTests
{
    private const int Eggel = 101;

    [Fact]
    public async Task The_screen_is_read_with_players_named_and_the_staff_lines_mapped()
    {
        var instructions = (await Client(new ScreenPlugin()).GetAsync())!;

        Assert.Equal((324, "GRE"), (instructions.TeamId, instructions.TeamName));
        var row = Assert.Single(instructions.Players);
        Assert.Equal((Eggel, "Marcelo Eggel", "AMC", "always", "hard"), (row.PlayerId, row.Name, row.Position?.Code, row.Marking, row.Tackling));
        Assert.Equal("none", row.Pressing);
        Assert.Equal("DCL", Assert.Single(instructions.Positions).Position?.Code);
        var suggestion = Assert.Single(instructions.StaffSuggestions);
        Assert.Equal((Eggel, OppositionInstruction.Pressing, "always"), (suggestion.PlayerId, suggestion.Instruction, suggestion.Value));
    }

    [Fact]
    public async Task A_change_is_set_on_the_tactic_for_the_players_row_as_the_screen_selects_it()
    {
        var plugin = new ScreenPlugin();

        await Client(plugin).SetAsync(Eggel, OppositionInstruction.Pressing, "always");

        var write = Uri.UnescapeDataString(Assert.Single(plugin.Writes).Query);
        Assert.StartsWith("/set", Assert.Single(plugin.Writes).AbsolutePath);
        Assert.Contains("base=Team.Tactic", write);
        Assert.Contains("ctx=TeamSelectionIndex:8", write);
        Assert.Contains("ctx=PositionMask:8192", write);
        Assert.Contains("ref=SelectedObjectList:Team.Tactic.NextOppositionInstructions.OppositionPredictedLineupInstructions[Index=8]", write);
        Assert.Contains("path=OppositionPlayerPressingTrigger", write);
        Assert.Contains("value=2", write);
    }

    [Fact]
    public async Task A_player_no_longer_in_the_expected_lineup_is_refused_and_nothing_is_sent()
    {
        var plugin = new ScreenPlugin();

        await Assert.ThrowsAsync<GameRejectedException>(() => Client(plugin).SetAsync(999, OppositionInstruction.Marking, "always"));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task Rows_the_game_didnt_send_are_unknown_not_empty()
    {
        var plugin = new ScreenPlugin(screen: """{"values":{"NextOppositionInstructionsTeam.UniqueId":324},"pending":["NextOppositionInstructions"]}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => Client(plugin).GetAsync());
    }

    [Fact]
    public async Task Without_a_match_ahead_there_are_no_instructions()
    {
        var plugin = new ScreenPlugin(screen: """{"values":{"NextOppositionInstructionsTeam.UniqueId":null,"NextOppositionInstructions":null}}""");

        Assert.Null(await Client(plugin).GetAsync());
    }

    private static OppositionInstructionsBridgeClient Client(ScreenPlugin plugin)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        return new OppositionInstructionsBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
    }

    // The plugin as the client sees it: Marcelo Eggel (record 12534) at AMC, eighth in the expected lineup, with tight
    // marking and hard tackling; one position row; one staff line about pressing him.
    private sealed class ScreenPlugin(string? screen = null) : HttpMessageHandler
    {
        private const string Screen = """
            {"values":{
              "NextOppositionInstructionsTeam.UniqueId":324,"NextOppositionInstructionsTeam.Name":"GRE",
              "NextOppositionInstructions":{
                "OppositionPredictedLineupInstructions":[{"player":{"$ref":"Person","index":12534},"Position":8192,"Index":8,
                  "OppositionPlayerMarking":2,"OppositionPlayerPressingTrigger":0,"OppositionPlayerTackling":3,"OppositionPlayerShowOnToFoot":0}],
                "OppositionAllPositionsInstructions":[{"player":{"$type":"SI.Bindable.NullReference"},"Position":1048592,"Index":-1,
                  "OppositionPlayerMarking":0,"OppositionPlayerPressingTrigger":0,"OppositionPlayerTackling":0,"OppositionPlayerShowOnToFoot":0}]},
              "AllOppositionInstructionSuggestions":[{"FeedbackString":"Fazer pressão quando a bola for passada para Marcelo Eggel",
                "FeedbackType":253,"IsDisabled":false,"player":{"$ref":"Person","index":12534}}]}}
            """;

        private const string Squad = """{"count":1,"items":[{"source":{"$ref":"Person","index":12534},"values":{"UniqueId":101,"Name":"Marcelo Eggel"}}]}""";

        public List<Uri> Writes { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post)
            {
                Writes.Add(request.RequestUri!);
                return Answer("""{"changed":true}""");
            }

            var url = Uri.UnescapeDataString(request.RequestUri!.Query);
            return Answer(url.Contains("list=") ? Squad : screen ?? Screen);
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
