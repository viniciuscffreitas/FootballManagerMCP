using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Contracts;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class ContractsServiceTests
{
    private readonly IContractsGameClient _game = Substitute.For<IContractsGameClient>();
    private readonly ContractsService _contracts;

    public ContractsServiceTests()
    {
        _contracts = new ContractsService(_game);
    }

    [Fact]
    public async Task Every_first_team_contract_comes_back_as_the_game_shows_it()
    {
        Contracts(new SquadPlayerContract
        {
            PlayerId = 19338228, Name = "Gabriel Brazão", Wage = "R$80 mil p/s", EndDate = new DateOnly(2028, 12, 31),
            TimeRemaining = "2 anos, 291 dias", Type = "Contrato Profissional", AgreedPlayingTime = "Goleiro titular",
            ClausesCount = 2, HasRenewalOffer = false, StaffAdvice = "Não oferecer contrato"
        }, Contract(2, "Rodinei", 2027, 12, 31));

        var contracts = (await _contracts.GetContractsAsync()).Value;

        Assert.Equal(["Gabriel Brazão", "Rodinei"], contracts.Select(contract => contract.Name));
        var brazao = contracts[0];
        Assert.Equal(("R$80 mil p/s", new DateOnly(2028, 12, 31), "Goleiro titular", 2, "Não oferecer contrato"),
            (brazao.Wage, brazao.EndDate, brazao.AgreedPlayingTime, brazao.ClausesCount, brazao.StaffAdvice));
    }

    [Fact]
    public async Task Expiring_by_keeps_only_contracts_ending_on_or_before_the_date()
    {
        Contracts(Contract(1, "Ends before", 2026, 6, 30), Contract(2, "Ends that day", 2026, 12, 31),
            Contract(3, "Ends after", 2027, 12, 31), Contract(4, "No end date", null, null, null));

        var contracts = (await _contracts.GetContractsAsync("2026-12-31")).Value;

        Assert.Equal(["Ends before", "Ends that day"], contracts.Select(contract => contract.Name));
    }

    [Fact]
    public async Task A_date_that_isnt_yyyy_MM_dd_is_refused_before_asking_the_game()
    {
        var result = await _contracts.GetContractsAsync("31/12/2026");

        Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        await _game.DidNotReceive().GetContractsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetContractsAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _contracts.GetContractsAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Contracts(params SquadPlayerContract[] contracts)
    {
        _game.GetContractsAsync(Arg.Any<CancellationToken>()).Returns([.. contracts]);
    }

    private static SquadPlayerContract Contract(int id, string name, int? year, int? month, int? day)
    {
        return new SquadPlayerContract
        {
            PlayerId = id, Name = name, EndDate = year is null ? null : new DateOnly(year.Value, month!.Value, day!.Value)
        };
    }
}
