using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Conversations;
using FootballManager.Domain.Saves;
using NSubstitute;

namespace FootballManager.Tests;

public class ConversationServiceTests
{
    private const int Rollheiser = 7;
    private const PlayerConversation Praise = PlayerConversation.PraisePlayerTrainingDevelopment;

    private readonly IConversationGameClient _game = Substitute.For<IConversationGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly ConversationService _conversations;

    public ConversationServiceTests()
    {
        _conversations = new ConversationService(_game, _saveGuard);
        _game.GetPlayerAsync(Rollheiser, Arg.Any<CancellationToken>())
            .Returns(new ConversationPlayer { Id = Rollheiser, Name = "Agustín Rollheiser", Available = [Praise] });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _game.EndAsync(Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public async Task Lists_the_conversations_the_players_menu_offers()
    {
        var player = (await _conversations.GetConversationsAsync(Rollheiser)).Value;

        Assert.Equal([Praise], player.Available);
    }

    [Fact]
    public async Task A_player_outside_the_first_team_is_not_found()
    {
        var result = await _conversations.TalkAsync(99, Praise, [], continueOpen: false, apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_conversation_the_menu_does_not_offer_is_refused_and_nothing_opens(bool apply)
    {
        var result = await _conversations.TalkAsync(Rollheiser, PlayerConversation.WarnPlayerConduct, [], continueOpen: false, apply);

        Assert.Contains("PraisePlayerTrainingDevelopment", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenAsync(default, default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task Opening_needs_a_conversation()
    {
        var result = await _conversations.TalkAsync(Rollheiser, null, [], continueOpen: false, apply: false);

        Assert.Contains("conversation", Messages(result));
    }

    [Fact]
    public async Task The_dry_run_warns_that_opening_uses_the_conversation_up_and_opens_nothing()
    {
        var talk = (await _conversations.TalkAsync(Rollheiser, Praise, [0], continueOpen: false, apply: false)).Value;

        Assert.False(talk.Applied);
        Assert.Equal(Praise, talk.Conversation);
        Assert.Contains(talk.Notes, note => note.Contains("closing it without an answer"));
        await _game.DidNotReceiveWithAnyArgs().OpenAsync(default, default);
        await _game.DidNotReceiveWithAnyArgs().SayAsync(default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_opens_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _conversations.TalkAsync(Rollheiser, Praise, [0], continueOpen: false, apply: true);

        Assert.Contains("Another save is loaded.", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenAsync(default, default);
    }

    [Fact]
    public async Task A_card_already_open_is_not_opened_over()
    {
        _game.ReadCardAsync(Arg.Any<CancellationToken>()).Returns(Card(["Você está treinando bem."]));

        var result = await _conversations.TalkAsync(Rollheiser, Praise, [0], continueOpen: false, apply: true);

        Assert.Contains("continue", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().OpenAsync(default, default);
    }

    [Fact]
    public async Task Applying_says_the_chosen_line_reads_the_reaction_and_ends_the_conversation()
    {
        _game.OpenAsync(Rollheiser, Praise, Arg.Any<CancellationToken>())
            .Returns(Card(["Você está treinando bem.", "Seu treino está excelente."]));
        _game.SayAsync(1, Arg.Any<CancellationToken>())
            .Returns(Card([], "Satisfeito", "Obrigado, eu tenho trabalhado muito."));

        var talk = (await _conversations.TalkAsync(Rollheiser, Praise, [1], continueOpen: false, apply: true)).Value;

        var round = Assert.Single(talk.Rounds);
        Assert.Equal("Seu treino está excelente.", round.ChosenLine);
        Assert.Equal(["Obrigado, eu tenho trabalhado muito."], round.Reaction);
        Assert.Equal("Satisfeito", round.BodyLanguage);
        Assert.True(talk.Ended);
        await _game.Received(1).EndAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Without_choices_the_card_is_left_open_with_its_lines_for_the_next_call()
    {
        _game.OpenAsync(Rollheiser, Praise, Arg.Any<CancellationToken>())
            .Returns(Card(["Você está treinando bem.", "Seu treino está excelente."]));

        var talk = (await _conversations.TalkAsync(Rollheiser, Praise, [], continueOpen: false, apply: true)).Value;

        Assert.Equal(["Você está treinando bem.", "Seu treino está excelente."], talk.Lines);
        Assert.False(talk.Ended);
        await _game.DidNotReceiveWithAnyArgs().SayAsync(default);
        await _game.DidNotReceiveWithAnyArgs().EndAsync();
    }

    [Fact]
    public async Task Continuing_plays_the_criticisms_second_round_and_ends()
    {
        _game.ReadCardAsync(Arg.Any<CancellationToken>()).Returns(Card(["Seu treino tem sido fraco.", "Espero mais de você."]));
        _game.SayAsync(0, Arg.Any<CancellationToken>())
            .Returns(Card(["Recuar: talvez eu tenha exagerado.", "Convencer: você sabe que pode mais.", "Ameaçar: vai para o banco."], "Abatido", "Eu discordo."));
        _game.SayAsync(1, Arg.Any<CancellationToken>()).Returns(Card([], "Satisfeito", "Vou me esforçar."));

        var talk = (await _conversations.TalkAsync(Rollheiser, null, [0, 1], continueOpen: true, apply: true)).Value;

        Assert.Equal(["Seu treino tem sido fraco.", "Convencer: você sabe que pode mais."], talk.Rounds.Select(round => round.ChosenLine));
        Assert.Equal("Satisfeito", talk.Rounds[1].BodyLanguage);
        Assert.True(talk.Ended);
        await _game.DidNotReceiveWithAnyArgs().OpenAsync(default, default);
        await _saveGuard.Received(1).PrepareWriteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Continuing_needs_an_open_card()
    {
        var result = await _conversations.TalkAsync(Rollheiser, null, [0], continueOpen: true, apply: true);

        Assert.Contains("No conversation card is open", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SayAsync(default);
    }

    [Fact]
    public async Task A_line_the_round_does_not_have_is_not_said()
    {
        _game.ReadCardAsync(Arg.Any<CancellationToken>()).Returns(Card(["Você está treinando bem."]));

        var talk = (await _conversations.TalkAsync(Rollheiser, null, [3], continueOpen: true, apply: true)).Value;

        Assert.Equal(["Você está treinando bem."], talk.Lines);
        Assert.Contains(talk.Notes, note => note.Contains("no line 3"));
        await _game.DidNotReceiveWithAnyArgs().SayAsync(default);
        await _game.DidNotReceiveWithAnyArgs().EndAsync();
    }

    private static ConversationCard Card(List<string> lines, string? bodyLanguage = null, string? answer = null)
    {
        var texts = new List<string> { "Agustín Rollheiser" };

        if (answer is not null)
        {
            texts.Add(answer);
        }

        return new ConversationCard { Lines = lines, BodyLanguage = bodyLanguage, Texts = texts };
    }

    private static string Messages<T>(Result<T> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
