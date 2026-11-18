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
}
