using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Transfers;
using NSubstitute;

namespace FootballManager.Tests;

public class ContractNegotiationTests
{
    private const int Walter = 301344;
    private const int Accepted = 812;

    private readonly ITransferGameClient _game = Substitute.For<ITransferGameClient>();
    private readonly IInboxGameClient _inbox = Substitute.For<IInboxGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly TransferService _transfers;

    private static readonly ContractPanel Promises = Panel([ContractStep.FinalisePromises],
        [new NegotiationTerm { Label = "Tempo de jogo", Value = "Goleiro titular" }]);

    private static readonly ContractPanel Moving = Panel([ContractStep.NegotiateContract],
        [new NegotiationTerm { Label = "Tempo de jogo", Value = "Goleiro titular" }], "Fico feliz em ter resolvido as preliminares.");

    private static readonly ContractPanel Contract = Panel([ContractStep.CloseDeal],
    [
        new NegotiationTerm { Label = "Salário", Value = "R$65.000 p/s | 65000" },
        new NegotiationTerm { Label = "Duração do contrato", Value = "10 meses" }
    ]);

    public ContractNegotiationTests()
    {
        _transfers = new TransferService(_game, _inbox, _saveGuard);
        _game.FindPlayerAsync(Walter, Arg.Any<CancellationToken>())
            .Returns(new TransferTarget { Id = Walter, Name = "Walter", Club = "Mirassol" });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _inbox.GetInboxAsync(Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new Inbox
        {
            Messages =
            [
                new InboxMessage { Id = 700, Title = "Proposta por Daniel Borges negociada", CanReply = true },
                new InboxMessage { Id = Accepted, Title = "Proposta Santos por Walter aceita", CanReply = true }
            ]
        });
        _inbox.GetMessageOptionsAsync(Accepted, Arg.Any<CancellationToken>()).Returns(new MessageOptions
        {
            MessageId = Accepted,
            Title = "Proposta Santos por Walter aceita",
            Options =
            [
                new MessageOption { Index = 0, Title = "Iniciar negociação de contrato", NewsAction = 7 },
                new MessageOption { Index = 1, Title = "Passar para Alexandre Mattos", Event = 27, ResponseId = 2, ResponseValue = 49 }
            ]
        });
        _game.OpenContractPanelAsync(Accepted, 0, Arg.Any<CancellationToken>()).Returns(Promises);
        _game.PressAsync(ContractStep.FinalisePromises, Arg.Any<CancellationToken>()).Returns(Moving);
        _game.PressAsync(ContractStep.NegotiateContract, Arg.Any<CancellationToken>()).Returns(Contract);
        _game.PressAsync(ContractStep.CloseDeal, Arg.Any<CancellationToken>()).Returns(Panel([], [], "Walter aceita os termos."));
    }

    [Fact]
    public async Task The_dry_run_finds_the_accepted_offers_message_and_opens_nothing()
    {
        var negotiation = (await _transfers.NegotiateContractAsync(Walter, apply: false)).Value;

        Assert.False(negotiation.Applied);
        Assert.Equal(Accepted, negotiation.MessageId);
        Assert.Equal("Proposta Santos por Walter aceita", negotiation.MessageTitle);
        await _game.DidNotReceiveWithAnyArgs().OpenContractPanelAsync(default, default);
        await _game.DidNotReceiveWithAnyArgs().PressAsync(default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task The_dry_run_on_the_contracts_page_shows_its_salary_and_length_and_presses_nothing()
    {
        _game.ReadContractPanelAsync(Arg.Any<CancellationToken>()).Returns(Contract);

        var negotiation = (await _transfers.NegotiateContractAsync(Walter, apply: false)).Value;

        Assert.Equal(["Salário", "Duração do contrato"], negotiation.Terms.Select(term => term.Label));
        Assert.Equal("R$65.000 p/s | 65000", negotiation.Terms[0].Value);
        Assert.Contains(negotiation.Notes, note => note.Contains("these terms"));
        await _game.DidNotReceiveWithAnyArgs().PressAsync(default);
    }

    [Fact]
    public async Task A_player_no_list_holds_is_not_found()
    {
        var result = await _transfers.NegotiateContractAsync(99, apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    [Fact]
    public async Task Without_an_accepted_offer_nothing_opens()
    {
        _inbox.GetInboxAsync(Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new Inbox());

        var result = await _transfers.NegotiateContractAsync(Walter, apply: true);

        Assert.Contains("make_transfer_offer", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenContractPanelAsync(default, default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task A_panel_open_for_another_player_is_not_pressed()
    {
        var other = Panel([ContractStep.CloseDeal], []);
        other.Player = "(Daniel Borges)";
        _game.ReadContractPanelAsync(Arg.Any<CancellationToken>()).Returns(other);

        var result = await _transfers.NegotiateContractAsync(Walter, apply: true);

        Assert.Contains("Daniel Borges", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().PressAsync(default);
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_opens_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _transfers.NegotiateContractAsync(Walter, apply: true);

        Assert.Contains("Another save is loaded.", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenContractPanelAsync(default, default);
        await _game.DidNotReceiveWithAnyArgs().PressAsync(default);
    }

    [Fact]
    public async Task Applying_opens_the_negotiation_goes_through_the_promises_and_stops_at_the_contracts_terms()
    {
        var negotiation = (await _transfers.NegotiateContractAsync(Walter, apply: true)).Value;

        Assert.Equal([ContractStep.FinalisePromises, ContractStep.NegotiateContract], negotiation.Steps);
        Assert.Equal("Salário", negotiation.Terms[0].Label);
        Assert.Contains("Fico feliz em ter resolvido as preliminares.", negotiation.Response);
        Assert.False(negotiation.Closed);
        await _game.DidNotReceive().PressAsync(ContractStep.CloseDeal, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Applying_on_the_contracts_page_closes_the_deal_at_its_terms()
    {
        _game.ReadContractPanelAsync(Arg.Any<CancellationToken>()).Returns(Contract);

        var negotiation = (await _transfers.NegotiateContractAsync(Walter, apply: true)).Value;

        Assert.Equal([ContractStep.CloseDeal], negotiation.Steps);
        Assert.True(negotiation.Closed);
        Assert.Contains("Walter aceita os termos.", negotiation.Response);
        await _game.DidNotReceiveWithAnyArgs().OpenContractPanelAsync(default, default);
    }

    [Fact]
    public async Task A_step_the_game_does_not_take_stops_the_run()
    {
        _game.PressAsync(ContractStep.FinalisePromises, Arg.Any<CancellationToken>()).Returns(Promises);

        var negotiation = (await _transfers.NegotiateContractAsync(Walter, apply: true)).Value;

        Assert.Equal([ContractStep.FinalisePromises], negotiation.Steps);
        Assert.Contains(negotiation.Notes, note => note.Contains("didn't move on"));
        await _game.DidNotReceive().PressAsync(ContractStep.NegotiateContract, Arg.Any<CancellationToken>());
    }

    private static ContractPanel Panel(List<ContractStep> steps, List<NegotiationTerm> terms, params string[] lines)
    {
        return new ContractPanel
        {
            Player = "(Walter)",
            Club = "Mirassol",
            Steps = steps,
            Terms = terms,
            Lines = [.. lines],
            Texts = ["Mirassol", "(Walter)", .. terms.Select(term => term.Label), .. lines]
        };
    }

    private static string Messages<T>(Result<T> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
