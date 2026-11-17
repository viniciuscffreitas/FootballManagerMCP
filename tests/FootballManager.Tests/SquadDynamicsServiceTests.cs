using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.DressingRoom;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class SquadDynamicsServiceTests
{
    private readonly ISquadDynamicsGameClient _game = Substitute.For<ISquadDynamicsGameClient>();
    private readonly SquadDynamicsService _dynamics;

    public SquadDynamicsServiceTests()
    {
        _dynamics = new SquadDynamicsService(_game);
    }

    [Fact]
    public async Task Hierarchy_atmosphere_issues_promises_and_players_come_back_together()
    {
        _game.GetSquadDynamicsAsync(Arg.Any<CancellationToken>()).Returns(new SquadDynamics
        {
            Hierarchy = new SquadHierarchy
            {
                TeamLeaders = [Member(1, "Neymar"), Member(2, "Gabriel Barbosa")],
                HighlyInfluential = [Member(3, "Gabriel Brazão")],
                Other = [Member(4, "Lima")]
            },
            Atmosphere = new DressingRoomAtmosphere
            {
                Summary = "Muito bom",
                Score = 80,
                Factors = [new AtmosphereFactor { Text = "O time tem muitas jogadoras que são amigas umas das outras.", Value = 2 }]
            },
            Issues =
            [
                new HappinessIssue
                {
                    PlayerId = 4, PlayerName = "Lima", Severity = "Preocupado",
                    Description = "Está insatisfeito com o tempo de jogo atual.", PlayersAgreeing = 0
                }
            ],
            Promises = [new PlayerPromise { Title = "Tempo de jogo", Status = "Em andamento", DaysRemaining = 30 }],
            Players =
            [
                new PlayerDynamics
                {
                    Id = 3, Name = "Gabriel Brazão", HierarchyLevel = "Jogador Altamente Influente",
                    HappinessPositives = ["Está satisfeito por ser considerado um jogador muito influente dentro do vestiário"],
                    HappinessNegatives = ["Não tem sentimentos fortes em relação ao apoio do elenco ao técnico"],
                    Promises = 1
                }
            ]
        });

        var dynamics = (await _dynamics.GetSquadDynamicsAsync()).Value;

        Assert.Equal(["Neymar", "Gabriel Barbosa"], dynamics.Hierarchy.TeamLeaders.Select(member => member.Name));
        Assert.Equal("Gabriel Brazão", Assert.Single(dynamics.Hierarchy.HighlyInfluential).Name);
        Assert.Empty(dynamics.Hierarchy.Influential);
        Assert.Equal(("Muito bom", 80), (dynamics.Atmosphere.Summary, dynamics.Atmosphere.Score));
        Assert.Equal(2, Assert.Single(dynamics.Atmosphere.Factors).Value);

        var issue = Assert.Single(dynamics.Issues);
        Assert.Equal(("Lima", "Preocupado", "Está insatisfeito com o tempo de jogo atual."), (issue.PlayerName, issue.Severity, issue.Description));

        Assert.Equal(("Tempo de jogo", 30), (Assert.Single(dynamics.Promises).Title, dynamics.Promises[0].DaysRemaining));

        var player = Assert.Single(dynamics.Players);
        Assert.Equal("Jogador Altamente Influente", player.HierarchyLevel);
        Assert.Single(player.HappinessPositives);
        Assert.Single(player.HappinessNegatives);
        Assert.Equal(1, player.Promises);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetSquadDynamicsAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _dynamics.GetSquadDynamicsAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private static HierarchyMember Member(int id, string name)
    {
        return new HierarchyMember { Id = id, Name = name };
    }
}
