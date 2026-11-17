using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Transfers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class TransferActivityServiceTests
{
    private readonly ITransferActivityGameClient _game = Substitute.For<ITransferActivityGameClient>();
    private readonly TransferActivityService _activity;

    public TransferActivityServiceTests()
    {
        _activity = new TransferActivityService(_game);
    }

    [Fact]
    public async Task The_counts_and_values_come_back_as_the_game_shows_them()
    {
        Readout(new TransferActivityReadout
        {
            Counts = new TransferOfferCounts { All = 3, In = 2, Out = 1, AwaitingResponseAll = 1, AwaitingResponseIn = 0, AwaitingResponseOut = 1, Trades = 0 },
            IncomingValue = "R$2,3 mi (R$2,5 mi)",
            OutgoingValue = "R$0 (R$0)"
        });

        var activity = (await _activity.GetTransferActivityAsync()).Value;

        Assert.Equal((3, 2, 1), (activity.Counts.All, activity.Counts.In, activity.Counts.Out));
        Assert.Equal((1, 0, 1), (activity.Counts.AwaitingResponseAll, activity.Counts.AwaitingResponseIn, activity.Counts.AwaitingResponseOut));
        Assert.Equal("R$2,3 mi (R$2,5 mi)", activity.IncomingValue);
        Assert.Equal("R$0 (R$0)", activity.OutgoingValue);
    }

    [Fact]
    public async Task Each_offer_names_the_other_club_of_the_deal()
    {
        Readout(new TransferActivityReadout
        {
            Players =
            [
                Player(1, "Gabriel Gorgulho", TransferDirection.In,
                    new TransferActivityOffer { BuyingClub = "Santos", SellingClub = "Fluminense", Type = "Transferência", Fee = "R$1,5 mi", Status = "Aguardando resposta" }),
                Player(2, "Sold", TransferDirection.Out,
                    new TransferActivityOffer { BuyingClub = "Palmeiras", SellingClub = "Santos", Type = "Empréstimo", Fee = "R$0", Status = "Proposta recebida", RequiresResponse = true })
            ]
        });

        var negotiations = (await _activity.GetTransferActivityAsync()).Value.Negotiations;

        var incoming = negotiations.Single(n => n.PlayerId == 1);
        Assert.Equal(("Gabriel Gorgulho", TransferDirection.In, "Fluminense", "Transferência", "R$1,5 mi", "Aguardando resposta"),
            (incoming.PlayerName, incoming.Direction, incoming.Club, incoming.Type, incoming.Fee, incoming.Status));

        var outgoing = negotiations.Single(n => n.PlayerId == 2);
        Assert.Equal(("Palmeiras", "Empréstimo", true), (outgoing.Club, outgoing.Type, outgoing.RequiresResponse));
    }

    [Fact]
    public async Task Offers_awaiting_the_users_answer_come_first()
    {
        Readout(new TransferActivityReadout
        {
            Players =
            [
                Player(1, "Waiting on them", TransferDirection.In, new TransferActivityOffer { SellingClub = "Mirassol" }),
                Player(2, "Waiting on us", TransferDirection.Out, new TransferActivityOffer { BuyingClub = "Bahia", RequiresResponse = true })
            ]
        });

        var negotiations = (await _activity.GetTransferActivityAsync()).Value.Negotiations;

        Assert.Equal([2, 1], negotiations.Select(n => n.PlayerId));
    }

    [Fact]
    public async Task A_player_whose_offers_the_game_didnt_list_still_shows_his_status()
    {
        Readout(new TransferActivityReadout
        {
            Players = [new TransferActivityPlayer { PlayerId = 7, Name = "Daniel Borges", Direction = TransferDirection.In, Status = "Proposta pendente", Wage = "R$40 mil p/m" }]
        });

        var negotiation = Assert.Single((await _activity.GetTransferActivityAsync()).Value.Negotiations);

        Assert.Equal(("Daniel Borges", "Proposta pendente", "R$40 mil p/m", (string?)null), (negotiation.PlayerName, negotiation.Status, negotiation.Wage, negotiation.Club));
    }

    [Fact]
    public async Task No_activity_is_an_empty_list()
    {
        Readout(new TransferActivityReadout { IncomingValue = "R$0 (R$0)", OutgoingValue = "R$0 (R$0)" });

        var activity = (await _activity.GetTransferActivityAsync()).Value;

        Assert.Empty(activity.Negotiations);
        Assert.Equal(0, activity.Counts.All);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetTransferActivityAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _activity.GetTransferActivityAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Readout(TransferActivityReadout readout)
    {
        _game.GetTransferActivityAsync(Arg.Any<CancellationToken>()).Returns(readout);
    }

    private static TransferActivityPlayer Player(int id, string name, TransferDirection direction, params TransferActivityOffer[] offers)
    {
        return new TransferActivityPlayer { PlayerId = id, Name = name, Direction = direction, Offers = [.. offers] };
    }
}
