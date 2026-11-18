using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Conversations;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Tactics.Editing;
using NSubstitute;

namespace FootballManager.Tests;

public class LiveMatchControlServiceTests
{
    private const int Santos = 335;
    private const int Neymar = 10;
    private const int Rollheiser = 7;
    private const int Coutinho = 23;

    private readonly ILiveMatchGameClient _live = Substitute.For<ILiveMatchGameClient>();
    private readonly ITacticEditorGameClient _tactics = Substitute.For<ITacticEditorGameClient>();
    private readonly IMatchesGameClient _matches = Substitute.For<IMatchesGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly LiveMatchControlService _control;

    private static readonly MatchPlace NeymarPlace = new() { Index = 9, PlayerId = Neymar, PlayerName = "Neymar", PositionMask = 2048 };
    private static readonly MatchPlace RollheiserPlace = new() { Index = 7, PlayerId = Rollheiser, PlayerName = "Rollheiser", PositionMask = 512 };
    private static readonly MatchPlace CoutinhoPlace = new() { Index = 13, PlayerId = Coutinho, PlayerName = "Coutinho" };

    public LiveMatchControlServiceTests()
    {
        _control = new LiveMatchControlService(_live, _tactics, new MatchesService(_matches), _saveGuard);

        _live.GetSideAsync(Arg.Any<CancellationToken>()).Returns(new LiveSide { IsHome = true, SubstitutionsRemaining = 5 });
        _live.GetPlacesAsync(true, Arg.Any<CancellationToken>()).Returns([NeymarPlace, RollheiserPlace, CoutinhoPlace]);
        _live.GetMentalitiesAsync(Arg.Any<CancellationToken>()).Returns(["Cautious", "Balanced", "Positive", "Attacking"]);
        _live.GetRolesAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(["InsideForward", "Winger"]);
        _live.GetTacticPlayerAsync(7, Arg.Any<CancellationToken>()).Returns(Rollheiser);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _matches.GetLiveMatchAsync(Arg.Any<CancellationToken>()).Returns(new LiveMatchReadout
        {
            Minute = 52,
            TeamId = Santos,
            Home = new MatchSide { TeamId = Santos, TeamName = "Santos" },
            Away = new MatchSide { TeamId = 318, TeamName = "Mirassol" },
            Tactic = new LiveTactic { Mentality = "Ofensivo" },
            SubstitutionsRemaining = 4
        });
    }

    [Fact]
    public async Task Out_of_a_match_every_change_is_refused_and_nothing_is_written()
    {
        _live.GetSideAsync(Arg.Any<CancellationToken>()).Returns((LiveSide?)null);

        Assert.Contains("No match", Messages(await _control.SubstituteAsync(Neymar, Coutinho, apply: true)));
        Assert.Contains("No match", Messages(await _control.SetMentalityAsync("Attacking", apply: true)));
        Assert.Contains("No match", Messages(await _control.SetRoleAsync(Rollheiser, true, "Winger", apply: true)));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task The_substitution_dry_run_names_who_goes_off_and_on_and_writes_nothing()
    {
        var action = (await _control.SubstituteAsync(Neymar, Coutinho, apply: false)).Value;

        Assert.False(action.Applied);
        Assert.Equal("Neymar off, Coutinho on (place 9)", action.Change);
        Assert.Contains(action.Notes, note => note.Contains("next stoppage"));
        await _live.DidNotReceiveWithAnyArgs().SubstituteAsync(default, default!, default!);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task An_applied_substitution_swaps_the_places_after_the_save_guard_and_reads_the_match_again()
    {
        _live.SubstituteAsync(true, NeymarPlace, CoutinhoPlace, Arg.Any<CancellationToken>())
            .Returns(new MatchPlace { Index = 9, PlayerId = Coutinho, PlayerName = "Coutinho" });

        var action = (await _control.SubstituteAsync(Neymar, Coutinho, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _live.SubstituteAsync(true, NeymarPlace, CoutinhoPlace, Arg.Any<CancellationToken>());
        });
        Assert.True(action.Applied);
        Assert.Equal(("Neymar", "Coutinho"), (action.Before, action.After));
        Assert.Equal((52, 4), (action.Match!.Minute, action.Match.SubstitutionsRemaining));
    }

    [Fact]
    public async Task A_substitution_the_tactic_did_not_take_is_said()
    {
        _live.SubstituteAsync(true, NeymarPlace, CoutinhoPlace, Arg.Any<CancellationToken>()).Returns(NeymarPlace);

        var action = (await _control.SubstituteAsync(Neymar, Coutinho, apply: true)).Value;

        Assert.Contains(action.Notes, note => note.Contains("didn't take the substitution"));
    }

    [Theory]
    [InlineData(Coutinho, Neymar, "isn't on the pitch")]
    [InlineData(Neymar, Rollheiser, "isn't on the bench")]
    [InlineData(99, Coutinho, "isn't on the pitch")]
    public async Task A_substitution_needs_a_player_on_the_pitch_and_one_on_the_bench(int off, int on, string reason)
    {
        var result = await _control.SubstituteAsync(off, on, apply: true);

        Assert.Contains(reason, Messages(result));
        await _live.DidNotReceiveWithAnyArgs().SubstituteAsync(default, default!, default!);
    }

    [Fact]
    public async Task No_substitution_is_made_once_the_team_has_none_left()
    {
        _live.GetSideAsync(Arg.Any<CancellationToken>()).Returns(new LiveSide { IsHome = true, SubstitutionsRemaining = 0 });

        Assert.Contains("no substitutions left", Messages(await _control.SubstituteAsync(Neymar, Coutinho, apply: true)));
    }

    [Fact]
    public async Task A_write_the_save_guard_refuses_changes_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>()).Returns(Result.Fail<SaveClearance>(new SaveRefusedError("wrong save")));

        var result = await _control.SubstituteAsync(Neymar, Coutinho, apply: true);

        Assert.True(result.HasError<SaveRefusedError>());
        await _live.DidNotReceiveWithAnyArgs().SubstituteAsync(default, default!, default!);
    }

    [Fact]
    public async Task A_mentality_the_game_does_not_have_is_refused_with_the_ones_it_has()
    {
        var message = Messages(await _control.SetMentalityAsync("AllOut", apply: true));

        Assert.Contains("Cautious, Balanced, Positive, Attacking", message);
        await _tactics.DidNotReceiveWithAnyArgs().SetMentalityAsync(default!);
    }

    [Fact]
    public async Task An_applied_mentality_shows_it_before_and_after_and_the_match_read_again()
    {
        _tactics.SetMentalityAsync("Attacking", Arg.Any<CancellationToken>()).Returns(new WriteOutcome("Positive", "Attacking"));

        var action = (await _control.SetMentalityAsync("Attacking", apply: true)).Value;

        Assert.Equal(("Positive", "Attacking"), (action.Before, action.After));
        Assert.Equal("Ofensivo", action.Match!.Tactic!.Mentality);
        await _saveGuard.Received(1).PrepareWriteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_mentality_dry_run_writes_nothing()
    {
        var action = (await _control.SetMentalityAsync("Attacking", apply: false)).Value;

        Assert.False(action.Applied);
        await _tactics.DidNotReceiveWithAnyArgs().SetMentalityAsync(default!);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task A_role_goes_to_the_players_place_on_the_pitch()
    {
        _tactics.SetRoleAsync(7, true, "InsideForward", Arg.Any<CancellationToken>()).Returns(new WriteOutcome("Winger", "InsideForward"));

        var action = (await _control.SetRoleAsync(Rollheiser, inPossession: true, "InsideForward", apply: true)).Value;

        Assert.Equal(("Winger", "InsideForward"), (action.Before, action.After));
        Assert.NotNull(action.Match);
    }

    [Fact]
    public async Task A_role_is_not_written_where_the_teams_tactic_has_someone_else()
    {
        _live.GetTacticPlayerAsync(7, Arg.Any<CancellationToken>()).Returns(Coutinho);

        var result = await _control.SetRoleAsync(Rollheiser, inPossession: true, "Winger", apply: true);

        Assert.Contains("nothing was written", Messages(result));
        await _tactics.DidNotReceiveWithAnyArgs().SetRoleAsync(default, default, default!);
    }

    [Fact]
    public async Task A_role_needs_a_player_on_the_pitch_and_a_role_the_game_has()
    {
        Assert.Contains("isn't on the pitch", Messages(await _control.SetRoleAsync(Coutinho, true, "Winger", apply: true)));
        Assert.Contains("no in possession role", Messages(await _control.SetRoleAsync(Rollheiser, true, "Libero", apply: true)));
    }

    [Fact]
    public async Task The_team_talk_is_only_read_while_it_is_on_screen()
    {
        _live.GetMatchDayStateAsync(Arg.Any<CancellationToken>()).Returns("Match");

        var result = await _control.TeamTalkAsync(null, apply: false);

        Assert.Contains("isn't on screen", Messages(result));
        await _live.DidNotReceiveWithAnyArgs().ReadTeamTalkAsync(default);
    }

    [Fact]
    public async Task The_team_talk_dry_run_lists_its_lines()
    {
        _live.GetMatchDayStateAsync(Arg.Any<CancellationToken>()).Returns("TeamTalk");
        _live.ReadTeamTalkAsync(Arg.Any<CancellationToken>()).Returns(new ConversationCard { Lines = ["Keep it up.", "Calm down."] });

        var action = (await _control.TeamTalkAsync(null, apply: false)).Value;

        Assert.Equal(["Keep it up.", "Calm down."], action.Lines);
        await _live.DidNotReceiveWithAnyArgs().SayTeamTalkAsync(default);
    }

    [Fact]
    public async Task An_applied_team_talk_says_the_line_and_shows_the_reaction()
    {
        _live.GetMatchDayStateAsync(Arg.Any<CancellationToken>()).Returns("FullTimeTeamTalk");
        _live.ReadTeamTalkAsync(Arg.Any<CancellationToken>())
            .Returns(new ConversationCard { Lines = ["Keep it up.", "Calm down."], Texts = ["Preleção"] });
        _live.SayTeamTalkAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ConversationCard { Texts = ["Preleção", "Os jogadores parecem motivados"] });

        var action = (await _control.TeamTalkAsync(1, apply: true)).Value;

        Assert.Equal("Calm down.", action.Said);
        Assert.Equal(["Os jogadores parecem motivados"], action.Reaction);
        await _saveGuard.Received(1).PrepareWriteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_team_talk_line_the_panel_does_not_have_is_refused()
    {
        _live.GetMatchDayStateAsync(Arg.Any<CancellationToken>()).Returns("TeamTalk");
        _live.ReadTeamTalkAsync(Arg.Any<CancellationToken>()).Returns(new ConversationCard { Lines = ["Keep it up."] });

        Assert.Contains("no line 3", Messages(await _control.TeamTalkAsync(3, apply: true)));
        Assert.Contains("Name the line", Messages(await _control.TeamTalkAsync(null, apply: true)));
        await _live.DidNotReceiveWithAnyArgs().SayTeamTalkAsync(default);
    }

    private static string Messages<T>(Result<T> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
