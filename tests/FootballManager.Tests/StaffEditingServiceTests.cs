using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Staff;
using NSubstitute;

namespace FootballManager.Tests;

public class StaffEditingServiceTests
{
    private const int Coach = 7;

    private readonly IStaffEditorGameClient _game = Substitute.For<IStaffEditorGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly StaffEditingService _staff;

    public StaffEditingServiceTests()
    {
        _game.FindMemberAsync(Coach, Arg.Any<CancellationToken>()).Returns(new StaffMember { Id = Coach, Name = "Eudes Pedro" });
        _game.GetOfferedActionsAsync(Coach, Arg.Any<CancellationToken>())
            .Returns([StaffMenuAction.Dismiss, StaffMenuAction.CoachingCourse]);
        _game.GetMenuAsync(Coach, StaffMenuAction.Dismiss, Arg.Any<CancellationToken>())
            .Returns(_ => [new MenuEntry { Label = "Demitir", Confirmation = "Pagar R$500 mil de indenização?" }]);
        _game.GetMenuAsync(Coach, StaffMenuAction.CoachingCourse, Arg.Any<CancellationToken>())
            .Returns(_ => [new MenuEntry
            {
                Label = "Curso de treinador",
                Children = [new MenuEntry { Label = "Licença B" }, new MenuEntry { Label = "Licença A" }]
            }]);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _staff = new StaffEditingService(_game, _saveGuard);
    }

    [Fact]
    public async Task The_dry_run_plans_the_entry_and_writes_nothing()
    {
        var plan = (await _staff.RunActionAsync(Coach, StaffMenuAction.Dismiss, null, apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal("Eudes Pedro", plan.StaffName);
        Assert.Equal("Demitir", plan.Choice);
        Assert.Equal("Pagar R$500 mil de indenização?", plan.Confirmation);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Applying_clears_the_save_then_picks_the_entry_and_reads_the_menu_back()
    {
        _game.GetOfferedActionsAsync(Coach, Arg.Any<CancellationToken>())
            .Returns(new HashSet<StaffMenuAction> { StaffMenuAction.Dismiss }, new HashSet<StaffMenuAction>());

        var plan = (await _staff.RunActionAsync(Coach, StaffMenuAction.Dismiss, null, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.RunMenuEntryAsync(Coach, StaffMenuAction.Dismiss,
                Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0 })), Arg.Any<CancellationToken>());
        });
        Assert.True(plan.Applied);
        Assert.False(plan.StillOffered);
        Assert.Contains(plan.Notes, note => note.Contains("MCPFM investigation"));
    }

    [Fact]
    public async Task An_entry_is_named_by_its_labels()
    {
        var plan = (await _staff.RunActionAsync(Coach, StaffMenuAction.CoachingCourse, ["curso de treinador", "licença a"], apply: true)).Value;

        Assert.Equal("Curso de treinador > Licença A", plan.Choice);
        await _game.Received(1).RunMenuEntryAsync(Coach, StaffMenuAction.CoachingCourse,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0, 1 })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_menu_with_several_entries_needs_one_named_to_apply()
    {
        var dryRun = (await _staff.RunActionAsync(Coach, StaffMenuAction.CoachingCourse, null, apply: false)).Value;
        var result = await _staff.RunActionAsync(Coach, StaffMenuAction.CoachingCourse, null, apply: true);

        Assert.Null(dryRun.Choice);
        Assert.Equal(2, dryRun.Menu[0].Children.Count);
        Assert.Contains("Curso de treinador > Licença B", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task An_action_the_menu_doesnt_offer_fails_clearly_and_writes_nothing()
    {
        var result = await _staff.RunActionAsync(Coach, StaffMenuAction.LeadershipCourse, null, apply: true);

        Assert.Contains("doesn't offer LeadershipCourse", Messages(result));
        Assert.Contains("Dismiss", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task An_entry_the_menu_doesnt_have_fails_with_the_entries_it_has()
    {
        var result = await _staff.RunActionAsync(Coach, StaffMenuAction.CoachingCourse, ["Curso de treinador", "Licença Pro"], apply: true);

        Assert.Contains("Licença Pro", Messages(result));
        Assert.Contains("Curso de treinador > Licença A", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Someone_not_on_the_first_teams_staff_is_refused()
    {
        var result = await _staff.RunActionAsync(99, StaffMenuAction.Dismiss, null, apply: false);

        Assert.Contains("not on the first team's staff", Messages(result));
    }

    [Fact]
    public async Task A_staff_members_action_needs_him_and_a_job_advert_takes_no_one()
    {
        Assert.Contains("staffId", Messages(await _staff.RunActionAsync(null, StaffMenuAction.Dismiss, null, apply: false)));
        Assert.Contains("club", Messages(await _staff.RunActionAsync(Coach, StaffMenuAction.AdvertiseCoachingJob, null, apply: false)));
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_sends_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _staff.RunActionAsync(Coach, StaffMenuAction.Dismiss, null, apply: true);

        Assert.Contains("Another save is loaded.", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task A_job_advert_runs_on_the_club()
    {
        _game.GetOfferedActionsAsync(null, Arg.Any<CancellationToken>()).Returns([StaffMenuAction.AdvertiseCoachingJob]);
        _game.GetMenuAsync(null, StaffMenuAction.AdvertiseCoachingJob, Arg.Any<CancellationToken>())
            .Returns([new MenuEntry { Label = "Anunciar vaga de treinador" }]);

        var plan = (await _staff.RunActionAsync(null, StaffMenuAction.AdvertiseCoachingJob, null, apply: true)).Value;

        Assert.True(plan.Applied);
        await _game.DidNotReceiveWithAnyArgs().FindMemberAsync(default);
        await _game.Received(1).RunMenuEntryAsync(null, StaffMenuAction.AdvertiseCoachingJob,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0 })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_apply_without_an_action_is_refused_and_sends_nothing()
    {
        var result = await _staff.RunActionAsync(Coach, null, null, apply: true);

        Assert.Contains("action", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task An_apply_whose_read_back_fails_still_reports_it_sent()
    {
        _game.GetOfferedActionsAsync(Coach, Arg.Any<CancellationToken>())
            .Returns(_ => new HashSet<StaffMenuAction> { StaffMenuAction.Dismiss }, _ => throw new GameUnavailableException("The game didn't answer."));

        var result = await _staff.RunActionAsync(Coach, StaffMenuAction.Dismiss, null, apply: true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Applied);
        Assert.Null(result.Value.StillOffered);
        Assert.Contains(result.Value.Notes, note => note.Contains("read back"));
    }

    private static string Messages<T>(Result<T> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" | ", result.Errors.SelectMany(error =>
            error is ValidationError validation ? validation.ErrorMessages.Prepend(validation.PropertyName) : [error.Message]));
    }
}
