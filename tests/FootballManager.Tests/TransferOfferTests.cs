using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Transfers;
using NSubstitute;

namespace FootballManager.Tests;

public class TransferOfferTests
{
    private const int Borges = 51;
    private const long Step = 10_000;

    private readonly ITransferGameClient _game = Substitute.For<ITransferGameClient>();
    private readonly IInboxGameClient _inbox = Substitute.For<IInboxGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly TransferService _transfers;

    // The fee the panel's field holds; each press of a button moves it a step.
    private long _fee = 680_000;
    private string _panelPlayer = "(Daniel Borges)";

    public TransferOfferTests()
    {
        _transfers = new TransferService(_game, _inbox, _saveGuard);
        _game.FindPlayerAsync(Borges, Arg.Any<CancellationToken>())
            .Returns(new TransferTarget { Id = Borges, Name = "Daniel Borges", Club = "Mirassol", CanMakeOffer = true });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _game.OpenOfferPanelAsync(Borges, Arg.Any<CancellationToken>()).Returns(_ => Panel());
        _game.StepFeeAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            _fee += call.Arg<bool>() ? Step : -Step;
            return Panel();
        });
        _game.SendOfferAsync(Arg.Any<CancellationToken>()).Returns(_ => Panel("Proposta atual"));
    }

    [Fact]
    public async Task The_dry_run_shows_the_player_his_club_and_the_fee_and_opens_nothing()
    {
        var offer = (await _transfers.MakeOfferAsync(Borges, 790_000, apply: false)).Value;

        Assert.False(offer.Applied);
        Assert.Equal("Daniel Borges", offer.PlayerName);
        Assert.Equal("Mirassol", offer.Club);
        Assert.Equal(790_000, offer.Fee);
        Assert.False(offer.Sent);
        await _game.DidNotReceiveWithAnyArgs().OpenOfferPanelAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SendOfferAsync();
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10_000)]
    public async Task A_fee_that_is_not_positive_is_refused(long fee)
    {
        var result = await _transfers.MakeOfferAsync(Borges, fee, apply: true);

        Assert.Contains("fee", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenOfferPanelAsync(default);
    }

    [Fact]
    public async Task A_player_no_list_holds_is_not_found()
    {
        var result = await _transfers.MakeOfferAsync(99, 790_000, apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_player_whose_menu_offers_no_transfer_offer_is_refused_and_nothing_opens(bool apply)
    {
        _game.FindPlayerAsync(Borges, Arg.Any<CancellationToken>())
            .Returns(new TransferTarget { Id = Borges, Name = "Daniel Borges", Club = "Santos", CanMakeOffer = false });

        var result = await _transfers.MakeOfferAsync(Borges, 790_000, apply);

        Assert.Contains("doesn't offer", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenOfferPanelAsync(default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_opens_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _transfers.MakeOfferAsync(Borges, 790_000, apply: true);

        Assert.Contains("Another save is loaded.", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenOfferPanelAsync(default);
    }

    [Fact]
    public async Task Applying_raises_the_fee_to_the_one_asked_and_sends_it()
    {
        var offer = (await _transfers.MakeOfferAsync(Borges, 790_000, apply: true)).Value;

        Assert.True(offer.Sent);
        Assert.Equal(680_000, offer.StartingFee);
        Assert.Equal(["Meu ponto de partida seria R$2.05 MI à vista."], offer.Advice);
        Assert.Equal(["Proposta atual"], offer.Response);
        await _game.Received(11).StepFeeAsync(true, Arg.Any<CancellationToken>());
        await _game.Received(1).SendOfferAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Applying_lowers_the_fee_when_the_panel_starts_above_it()
    {
        var offer = (await _transfers.MakeOfferAsync(Borges, 650_000, apply: true)).Value;

        Assert.True(offer.Sent);
        await _game.Received(3).StepFeeAsync(false, Arg.Any<CancellationToken>());
        await _game.DidNotReceive().StepFeeAsync(true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_fee_the_buttons_cannot_reach_exactly_is_not_sent()
    {
        var result = await _transfers.MakeOfferAsync(Borges, 795_000, apply: true);

        Assert.Contains("795000", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SendOfferAsync();
    }

    [Fact]
    public async Task A_fee_further_than_the_buttons_go_in_one_call_is_not_sent()
    {
        var result = await _transfers.MakeOfferAsync(Borges, 50_000_000, apply: true);

        Assert.Contains("press", Messages(result));
        await _game.Received(1).StepFeeAsync(true, Arg.Any<CancellationToken>());
        await _game.DidNotReceiveWithAnyArgs().SendOfferAsync();
    }

    [Fact]
    public async Task A_fee_the_game_stops_moving_is_not_sent()
    {
        _game.StepFeeAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(_ => Panel());

        var result = await _transfers.MakeOfferAsync(Borges, 790_000, apply: true);

        Assert.Contains("680000", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SendOfferAsync();
    }

    [Fact]
    public async Task A_panel_for_another_player_is_not_sent()
    {
        _panelPlayer = "(Walter)";

        var result = await _transfers.MakeOfferAsync(Borges, 790_000, apply: true);

        Assert.Contains("Walter", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().StepFeeAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SendOfferAsync();
    }

    [Fact]
    public async Task A_panel_that_does_not_show_sends_nothing()
    {
        _game.OpenOfferPanelAsync(Borges, Arg.Any<CancellationToken>()).Returns((OfferPanel?)null);

        var offer = (await _transfers.MakeOfferAsync(Borges, 790_000, apply: true)).Value;

        Assert.False(offer.Sent);
        Assert.Contains(offer.Notes, note => note.Contains("didn't show"));
        await _game.DidNotReceiveWithAnyArgs().SendOfferAsync();
    }

    private OfferPanel Panel(params string[] extra)
    {
        return new OfferPanel
        {
            Player = _panelPlayer,
            Club = "Mirassol",
            Fee = _fee,
            Advice = ["Meu ponto de partida seria R$2.05 MI à vista."],
            Texts = ["Mirassol", _panelPlayer, .. extra]
        };
    }

    private static string Messages<T>(Result<T> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
