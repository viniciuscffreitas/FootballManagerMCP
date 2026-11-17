using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Saves;
using NSubstitute;

namespace FootballManager.Tests;

public class InboxReplyTests
{
    private const int CodeOfConduct = 7;
    private const int Board = 5;

    private readonly IInboxGameClient _game = Substitute.For<IInboxGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly InboxService _inbox;

    public InboxReplyTests()
    {
        _game.GetMessageOptionsAsync(CodeOfConduct, Arg.Any<CancellationToken>()).Returns(_ => ConductMessage());
        _game.GetMessageOptionsAsync(Board, Arg.Any<CancellationToken>()).Returns(_ => BoardMessage());
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _inbox = new InboxService(_game, _saveGuard);
    }

    [Fact]
    public async Task The_options_say_which_ones_open_a_flow_and_how_many_answers_are_still_needed()
    {
        var options = (await _inbox.GetMessageOptionsAsync(Board)).Value;

        Assert.Equal(3, options.RequiredResponses);
        Assert.True(options.HasRequiredAction);
        Assert.Null(options.Options[0].Flow);
        Assert.Equal("negotiate the board's vision", options.Options[1].Flow);
        Assert.Equal("the club's vision screen", options.Options[2].Flow);
    }

    [Fact]
    public async Task A_message_the_inbox_doesnt_hold_is_not_found()
    {
        var result = await _inbox.GetMessageOptionsAsync(999);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    [Fact]
    public async Task A_dry_run_shows_the_option_and_sends_nothing()
    {
        var reply = (await _inbox.ReplyAsync(CodeOfConduct, 2, apply: false)).Value;

        Assert.False(reply.Applied);
        Assert.Equal("Pedir a Cuquinha para escolher", reply.Option.Title);
        Assert.Equal(3, reply.Option.ResponseValue);
        Assert.Equal(1, reply.RequiredResponses);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SendReplyAsync(default, default, default);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(-1)]
    public async Task An_option_the_message_doesnt_have_is_refused_and_nothing_is_sent(int option)
    {
        var result = await _inbox.ReplyAsync(CodeOfConduct, option, apply: true);

        Assert.Contains("0 Confirmar, 1 Recusar, 2 Pedir a Cuquinha para escolher", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SendReplyAsync(default, default, default);
    }

    [Fact]
    public async Task An_option_that_opens_a_flow_is_refused_with_what_it_opens()
    {
        var result = await _inbox.ReplyAsync(Board, 1, apply: true);

        Assert.Contains("negotiate the board's vision", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SendReplyAsync(default, default, default);
    }

    [Fact]
    public async Task A_disabled_option_is_refused()
    {
        var result = await _inbox.ReplyAsync(CodeOfConduct, 3, apply: true);

        Assert.Contains("disabled", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SendReplyAsync(default, default, default);
    }

    [Fact]
    public async Task An_apply_clears_the_save_sends_the_options_answer_and_reads_the_message_back()
    {
        var answered = ConductMessage();
        answered.HasReplied = true;
        answered.RequiredResponses = 0;
        _game.GetMessageOptionsAsync(CodeOfConduct, Arg.Any<CancellationToken>()).Returns(ConductMessage(), answered);

        var reply = (await _inbox.ReplyAsync(CodeOfConduct, 2, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.SendReplyAsync(CodeOfConduct, 1, 3, Arg.Any<CancellationToken>());
        });
        Assert.True(reply.Applied);
        Assert.True(reply.HasReplied);
        Assert.Equal(0, reply.RequiredResponses);
        Assert.Contains(reply.Notes, note => note.Contains("MCPFM investigation"));
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_sends_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _inbox.ReplyAsync(CodeOfConduct, 0, apply: true);

        Assert.Contains("Another save is loaded.", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SendReplyAsync(default, default, default);
    }

    [Fact]
    public async Task A_message_already_answered_is_refused()
    {
        var answered = ConductMessage();
        answered.HasReplied = true;
        _game.GetMessageOptionsAsync(CodeOfConduct, Arg.Any<CancellationToken>()).Returns(answered);

        var result = await _inbox.ReplyAsync(CodeOfConduct, 0, apply: true);

        Assert.Contains("already answered", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SendReplyAsync(default, default, default);
    }

    private static MessageOptions ConductMessage() => new()
    {
        MessageId = CodeOfConduct,
        Title = "Definir Código de Conduta",
        CanReply = true,
        HasRequiredAction = true,
        RequiredResponses = 1,
        Options =
        [
            new() { Index = 0, Title = "Confirmar", Event = 27, ResponseId = 1, ResponseValue = 1 },
            new() { Index = 1, Title = "Recusar", Event = 27, ResponseId = 1, ResponseValue = 2 },
            new() { Index = 2, Title = "Pedir a Cuquinha para escolher", Event = 27, ResponseId = 1, ResponseValue = 3 },
            new() { Index = 3, Title = "Desativado", Event = 27, ResponseId = 1, ResponseValue = 4, IsDisabled = true }
        ]
    };

    private static MessageOptions BoardMessage() => new()
    {
        MessageId = Board,
        Title = "Reunião de visão",
        CanReply = true,
        HasRequiredAction = true,
        RequiredResponses = 3,
        Options =
        [
            new() { Index = 0, Title = "Aceitar Visão Atual", Event = 27, ResponseId = 1, ResponseValue = 2 },
            new() { Index = 1, Title = "Negociar", Event = 0, NewsAction = 108 },
            new() { Index = 2, Title = "Ver visão do clube", Event = 0, NewsAction = 173 }
        ]
    };

    private static string Messages<T>(Result<T> result)
    {
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
