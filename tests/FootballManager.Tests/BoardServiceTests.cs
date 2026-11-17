using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Board;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class BoardServiceTests
{
    private readonly IBoardGameClient _game = Substitute.For<IBoardGameClient>();
    private readonly BoardService _board;

    public BoardServiceTests()
    {
        _board = new BoardService(_game);
    }

    [Fact]
    public async Task Confidence_vision_requests_and_promises_come_back_as_the_game_says_them()
    {
        Status(new BoardStatus
        {
            Board = new BoardConfidence
            {
                Grade = 6,
                Summary = "Resumindo, a diretoria está contente com a sua gestão do time.",
                Highlights = ["Encantados com o clima positivo no vestiário."],
                Criticism = ["Preocupados com a derrota por 0-2 para Corinthians."]
            },
            Supporters = new SupporterConfidence { Grade = 3, Summary = "Os torcedores estão contentes." },
            ThisYearObjective = new VisionObjective
            {
                Name = "Bater o recorde e terminar na metade de cima da tabela Brasileirão", Year = "Temporada atual", Progress = 6
            },
            FinalObjective = new VisionObjective { Name = "Desenvolver o melhor sistema de base do país", Year = "2030", Status = 0 },
            SupporterCulture = ["Jogar um futebol vistoso", "Jogar futebol ofensivo"],
            BoardRequests = 2,
            ActiveBoardRequests = 1,
            Promises = ["Win the league"]
        });

        var board = (await _board.GetBoardAsync()).Value;

        Assert.Equal((6, "Resumindo, a diretoria está contente com a sua gestão do time."), (board.Board.Grade, board.Board.Summary));
        Assert.Equal(["Encantados com o clima positivo no vestiário."], board.Board.Highlights);
        Assert.Equal(["Preocupados com a derrota por 0-2 para Corinthians."], board.Board.Criticism);
        Assert.Equal(3, board.Supporters.Grade);
        Assert.Equal("Bater o recorde e terminar na metade de cima da tabela Brasileirão", board.ThisYearObjective?.Name);
        Assert.Equal("2030", board.FinalObjective?.Year);
        Assert.Equal(["Jogar um futebol vistoso", "Jogar futebol ofensivo"], board.SupporterCulture);
        Assert.Equal((2, 1), (board.BoardRequests, board.ActiveBoardRequests));
        Assert.Equal(["Win the league"], board.Promises);
    }

    [Fact]
    public async Task No_promises_is_an_empty_list()
    {
        Status(new BoardStatus { Board = new BoardConfidence(), Supporters = new SupporterConfidence() });

        var result = await _board.GetBoardAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Promises);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetBoardAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _board.GetBoardAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Status(BoardStatus status)
    {
        _game.GetBoardAsync(Arg.Any<CancellationToken>()).Returns(status);
    }
}
