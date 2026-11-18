using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Board;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using NSubstitute;

namespace FootballManager.Tests;

public class BoardEditingServiceTests
{
    private const string Notice = "A diretoria entrará em contato com você em breve para comunicar a decisão.";

    private readonly IBoardEditorGameClient _game = Substitute.For<IBoardEditorGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly BoardEditingService _board;

    public BoardEditingServiceTests()
    {
        _game.GetOfferedActionsAsync(Arg.Any<CancellationToken>())
            .Returns([BoardMenuAction.Request, BoardMenuAction.IncreaseBudgets, BoardMenuAction.ManagerNewContract]);
        _game.GetMenuAsync(BoardMenuAction.IncreaseBudgets, Arg.Any<CancellationToken>())
            .Returns(_ => [
                new MenuEntry { Label = "Aumentar Orçamento de Salários", Confirmation = Notice },
                new MenuEntry { Label = "Aumentar orçamento de transferências", Confirmation = Notice }
            ]);
        _game.GetMenuAsync(BoardMenuAction.Request, Arg.Any<CancellationToken>())
            .Returns(_ => [new MenuEntry { Label = "Pedido à Diretoria" }]);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _board = new BoardEditingService(_game, _saveGuard);
    }

    [Fact]
    public async Task The_dry_run_lists_the_offered_requests_and_writes_nothing()
    {
        var plan = (await _board.RequestAsync(BoardMenuAction.IncreaseBudgets, "aumentar orçamento de salários", apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal("Aumentar Orçamento de Salários", plan.Choice);
        Assert.Equal(Notice, plan.Confirmation);
        Assert.Equal(2, plan.Menu.Count);
        Assert.Contains(BoardMenuAction.ManagerNewContract, plan.Offered);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default);
    }

    [Fact]
    public async Task Applying_clears_the_save_then_picks_the_entry_and_reads_the_talk_it_opens()
    {
        _game.ReadCardAsync(Arg.Any<CancellationToken>())
            .Returns(new BoardCard { Lines = ["Melhorar as instalações de base", "Contratar mais olheiros"] });

        var plan = (await _board.RequestAsync(BoardMenuAction.Request, null, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.RunMenuEntryAsync(BoardMenuAction.Request, 0, Arg.Any<CancellationToken>());
        });
        Assert.True(plan.Applied);
        Assert.Equal("Melhorar as instalações de base", plan.Card!.Lines[0]);
        Assert.Contains(plan.Notes, note => note.Contains("MCPFM investigation"));
    }

    [Fact]
    public async Task A_request_the_menu_does_not_offer_fails_with_the_ones_it_does()
    {
        var result = await _board.RequestAsync(BoardMenuAction.AffiliateLink, null, apply: true);

        var error = Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        Assert.Contains("IncreaseBudgets", Assert.Single(error.ErrorMessages));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default);
    }

    [Fact]
    public async Task An_entry_the_menu_does_not_have_fails_and_writes_nothing()
    {
        var result = await _board.RequestAsync(BoardMenuAction.IncreaseBudgets, "Comprar um estádio", apply: true);

        Assert.True(result.IsFailed);
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default);
    }

    [Fact]
    public async Task Applying_a_menu_with_several_entries_needs_one_named()
    {
        var result = await _board.RequestAsync(BoardMenuAction.IncreaseBudgets, null, apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task Answering_without_a_talk_open_fails_and_writes_nothing()
    {
        var result = await _board.AnswerAsync("0", apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SayAsync(default);
    }

    [Fact]
    public async Task The_answer_dry_run_shows_the_lines_and_picks_one_by_its_text()
    {
        _game.ReadCardAsync(Arg.Any<CancellationToken>())
            .Returns(new BoardCard { Lines = ["Aceitar Visão Atual", "Negociar"] });

        var plan = (await _board.AnswerAsync("negociar", apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal("Negociar", plan.Line);
        await _game.DidNotReceiveWithAnyArgs().SayAsync(default);
    }

    [Fact]
    public async Task Applying_an_answer_clears_the_save_then_says_the_line()
    {
        _game.ReadCardAsync(Arg.Any<CancellationToken>())
            .Returns(new BoardCard { Lines = ["Aceitar Visão Atual", "Negociar"] });
        _game.SayAsync(1, Arg.Any<CancellationToken>()).Returns(new BoardCard { Lines = ["Sem julgamento"] });

        var answer = (await _board.AnswerAsync("1", apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.SayAsync(1, Arg.Any<CancellationToken>());
        });
        Assert.True(answer.Applied);
        Assert.Equal("Negociar", answer.Line);
        Assert.Equal("Sem julgamento", answer.After!.Lines[0]);
    }

    [Fact]
    public async Task The_budget_dry_run_reads_the_menu_and_opens_nothing()
    {
        _game.GetBudgetsAsync(Arg.Any<CancellationToken>()).Returns("R$30 MI / R$52.5 mil p/s");

        var plan = (await _board.AdjustBudgetAsync(0.4f, apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal("R$30 MI / R$52.5 mil p/s", plan.Budgets);
        await _game.DidNotReceiveWithAnyArgs().OpenBudgetPanelAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SetBudgetSliderAsync(default);
    }

    [Fact]
    public async Task Applying_a_budget_split_clears_the_save_then_moves_the_slider()
    {
        _game.GetBudgetsAsync(Arg.Any<CancellationToken>()).Returns("R$30 MI / R$52.5 mil p/s");
        _game.OpenBudgetPanelAsync(Arg.Any<CancellationToken>())
            .Returns(new BudgetPanel { SliderValue = 0.5f, CanEdit = true, TransferBudget = "R$30 MI" });
        _game.SetBudgetSliderAsync(0.4f, Arg.Any<CancellationToken>())
            .Returns(new BudgetPanel { SliderValue = 0.4f, CanEdit = true, TransferBudget = "R$25 MI" });

        var plan = (await _board.AdjustBudgetAsync(0.4f, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.OpenBudgetPanelAsync(Arg.Any<CancellationToken>());
            _game.SetBudgetSliderAsync(0.4f, Arg.Any<CancellationToken>());
        });
        Assert.True(plan.Applied);
        Assert.Equal("R$30 MI", plan.Before!.TransferBudget);
        Assert.Equal("R$25 MI", plan.After!.TransferBudget);
    }

    [Fact]
    public async Task A_budget_the_board_does_not_let_the_user_edit_fails()
    {
        _game.GetBudgetsAsync(Arg.Any<CancellationToken>()).Returns("R$30 MI / R$52.5 mil p/s");
        _game.OpenBudgetPanelAsync(Arg.Any<CancellationToken>()).Returns(new BudgetPanel { SliderValue = 0.5f, CanEdit = false });

        var result = await _board.AdjustBudgetAsync(0.4f, apply: true);

        Assert.True(result.IsFailed);
        await _game.DidNotReceiveWithAnyArgs().SetBudgetSliderAsync(default);
    }

    [Fact]
    public async Task A_budget_adjustment_the_menu_does_not_offer_fails_before_writing()
    {
        _game.GetBudgetsAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        var result = await _board.AdjustBudgetAsync(0.4f, apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }
}
