using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Finances;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class FinancesServiceTests
{
    private readonly IFinancesGameClient _game = Substitute.For<IFinancesGameClient>();
    private readonly FinancesService _finances;

    public FinancesServiceTests()
    {
        _finances = new FinancesService(_game);
    }

    [Fact]
    public async Task The_games_own_texts_come_back_as_it_writes_them()
    {
        Readout(new FinanceReadout
        {
            FinancialStatus = "Instável",
            Wages = new WageSummary { Committed = "R$3.396.473 p/s", Budget = "R$3.486.460 p/s", Remaining = "R$89.987 p/s", NextSeasonBudget = "Ainda não anunciado" },
            IncomeLines = [new FinanceLine { Name = "Receita de TV", Amount = "R$8.01 MI", Percentage = 39, ChangeFromLastMonth = -31 }],
            ExpenditureLines = [new FinanceLine { Name = "Impostos", Amount = "R$962 mil", Percentage = 4, ChangeFromLastMonth = -9 }],
            NotableIncome = [new NotableIncome { Name = "Premiação", ThisMonth = "R$9.64 MI", ThisSeason = "R$9.64 MI", LastSeason = "R$0" }]
        });

        var finances = (await _finances.GetFinancesAsync()).Value;

        Assert.Equal("Instável", finances.FinancialStatus);
        Assert.Equal(("R$3.396.473 p/s", "R$3.486.460 p/s", "R$89.987 p/s", "Ainda não anunciado"),
            (finances.Wages.Committed, finances.Wages.Budget, finances.Wages.Remaining, finances.Wages.NextSeasonBudget));
        var income = Assert.Single(finances.IncomeLines);
        Assert.Equal(("Receita de TV", "R$8.01 MI", 39, -31), (income.Name, income.Amount, income.Percentage, income.ChangeFromLastMonth));
        Assert.Equal("R$962 mil", Assert.Single(finances.ExpenditureLines).Amount);
        Assert.Equal("R$9.64 MI", Assert.Single(finances.NotableIncome).ThisMonth);
    }

    [Fact]
    public async Task Amounts_the_game_sends_only_in_its_unit_are_converted_at_the_rate_its_wage_table_shows()
    {
        Readout(new FinanceReadout
        {
            Balance = -1000,
            TransferBudget = 500000,
            TransferBudgetNextSeason = 0,
            IncomeThisMonth = 99,
            WageGroups = [Wages("Geral", "R$1.000.000", 125000)],
            BalanceHistory = [new BalanceAt { When = "2026-01-01", Amount = 200000 }],
            BalanceProjection = [new BalanceAt { When = "Fim da temporada 2026", Amount = -2500000 }]
        });

        var finances = (await _finances.GetFinancesAsync()).Value;

        Assert.Equal(8m, finances.ConversionRate);
        Assert.Equal(("-R$8.000", "R$4.000.000", "R$0", "R$792"),
            (finances.Balance, finances.TransferBudget, finances.TransferBudgetNextSeason, finances.IncomeThisMonth));
        Assert.Null(finances.ExpenditureThisMonth);
        var month = Assert.Single(finances.BalanceHistory);
        Assert.Equal(("2026-01-01", "R$1.600.000"), (month.When, month.Balance));
        var seasonEnd = Assert.Single(finances.BalanceProjection);
        Assert.Equal(("Fim da temporada 2026", "-R$20.000.000"), (seasonEnd.When, seasonEnd.Balance));
    }

    [Fact]
    public async Task An_abbreviated_wage_total_doesnt_set_the_rate()
    {
        Readout(new FinanceReadout
        {
            Balance = 10,
            WageGroups = [Wages("Geral", "R$3.5 MI", 500000), Wages("Jovem", "R$1.000", 125)]
        });

        var finances = (await _finances.GetFinancesAsync()).Value;

        Assert.Equal(8m, finances.ConversionRate);
        Assert.Equal("R$80", finances.Balance);
    }

    [Fact]
    public async Task Without_a_wage_total_to_read_the_rate_from_amounts_stay_in_pounds()
    {
        Readout(new FinanceReadout { Balance = -7889929, WageGroups = [Wages("Geral", "R$0", 0)] });

        var finances = (await _finances.GetFinancesAsync()).Value;

        Assert.Null(finances.ConversionRate);
        Assert.Equal("-£7,889,929", finances.Balance);
    }

    [Fact]
    public async Task Kinds_of_player_the_squad_has_none_of_are_left_out()
    {
        Readout(new FinanceReadout
        {
            WageGroups = [Wages("Jogador Estrela", "R$0", 0, players: 0), Wages("Titular Regular", "R$810.224", 116563, players: 10)]
        });

        var finances = (await _finances.GetFinancesAsync()).Value;

        Assert.Equal("Titular Regular", Assert.Single(finances.WageGroups).Name);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetFinancesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _finances.GetFinancesAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Readout(FinanceReadout readout)
    {
        _game.GetFinancesAsync(Arg.Any<CancellationToken>()).Returns(readout);
    }

    private static WageGroup Wages(string name, string total, long inGameUnit, int players = 1)
    {
        return new WageGroup { Name = name, Players = players, TotalWages = total, TotalWagesInGameUnit = inGameUnit };
    }
}
