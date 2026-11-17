using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Availability;
using FootballManager.Domain.Players;
using FootballManager.Domain.Rotation;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using NSubstitute;

namespace FootballManager.Tests;

// A 4-2-3-1 with players 1-11 starting at slots 0-10, and reserves from 12. Every player is fit (condition 95,
// sharpness 80) and rated 10 at his own slot unless a test says otherwise. Picks change the eleven, as in the game.
public class RotationServiceTests
{
    private static readonly string[] Codes = ["GK", "DR", "DCR", "DCL", "DL", "DMR", "DML", "AMR", "AMC", "AML", "ST"];

    private readonly ISquadGameClient _squad = Substitute.For<ISquadGameClient>();
    private readonly IAvailabilityGameClient _availability = Substitute.For<IAvailabilityGameClient>();
    private readonly ITacticEditorGameClient _editor = Substitute.For<ITacticEditorGameClient>();
    private readonly ITacticsGameClient _tactics = Substitute.For<ITacticsGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly RotationService _rotation;

    private readonly Dictionary<int, Player> _players = [];
    private readonly List<PlayerInjury> _injuries = [];
    private readonly List<PlayerBan> _bans = [];
    private readonly NextMatchRules _nextMatch = new() { Competition = "Paulistão" };
    private readonly Dictionary<PitchPosition, Dictionary<int, int>> _ratings = [];
    private readonly int?[] _eleven = [.. Enumerable.Range(1, 11).Select(id => (int?)id)];

    public RotationServiceTests()
    {
        for (var id = 1; id <= 11; id++)
        {
            Player(id);
            Rate(Position(id - 1), (id, 10));
        }

        _squad.GetSquadAsync(Arg.Any<IReadOnlyCollection<AttributeDefinition>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => new Squad { TeamName = "Santos", Players = [.. _players.Values] });
        _availability.GetAvailabilityAsync(Arg.Any<CancellationToken>())
            .Returns(_ => new AvailabilityReadout { Injuries = [.. _injuries], Bans = [.. _bans], NextMatch = _nextMatch });

        _editor.GetOptionsAsync(Arg.Any<CancellationToken>()).Returns(new TacticOptions
        {
            ActiveSlot = 2,
            Roles = new ByPhase<List<string>>
            {
                InPossession = [.. Codes.Select((_, index) => $"Role{index}")],
                OutOfPossession = [.. Codes.Select((_, index) => $"Out{index}")]
            }
        });
        _editor.GetTacticAsync(Arg.Any<CancellationToken>()).Returns(_ => Tactic());
        _editor.GetRolesAsync(Arg.Any<int>(), Arg.Any<PitchPosition>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => Slots()
                .Where(slot => slot.Position.InPossession!.Position == call.ArgAt<PitchPosition>(1))
                .Select(slot => new RoleRating { Key = slot.Role.For(call.ArgAt<bool>(2)), Role = "Role", Rating = new Rating(10, 10) })
                .ToList());
        _editor.When(game => game.PickAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()))
            .Do(call => _eleven[call.ArgAt<int>(0)] = call.ArgAt<int>(1));
        _tactics.GetRoleFitAsync(Arg.Any<PitchPosition>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => RoleFit(call.ArgAt<PitchPosition>(0)));

        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", Club = "Santos" }));

        var tactics = new TacticEditingService(_editor, _saveGuard);
        _rotation = new RotationService(new AvailabilityService(_squad, _availability), _editor, _tactics, tactics);
    }

    [Fact]
    public async Task Fit_and_available_starters_all_stay()
    {
        Player(12);
        Rate(PitchPosition.ST, (12, 15));

        var plan = (await _rotation.SuggestAsync(apply: false)).Value;

        Assert.Empty(plan.Swaps);
        Assert.Empty(plan.Unfilled);
        Assert.Null(plan.Edit);
    }

    [Fact]
    public async Task An_injured_starter_goes_out_for_a_reserve_of_at_least_92_condition()
    {
        Injure(11);
        Player(12, condition: 91);
        Player(13, condition: 92);
        Rate(PitchPosition.ST, (12, 15), (13, 8));

        var plan = (await _rotation.SuggestAsync(apply: false)).Value;

        var swap = Assert.Single(plan.Swaps);
        Assert.Equal((10, "ST", "Role10", 11, 13), (swap.Index, swap.Position, swap.Role, swap.OutPlayerId, swap.InPlayerId));
        Assert.Contains("injured", swap.Reason);
    }

    [Fact]
    public async Task A_starter_banned_in_the_next_matchs_competition_goes_out()
    {
        _bans.Add(new PlayerBan { PlayerId = 6, Name = "Player 6", Scope = "Paulistão", MatchesRemaining = 1, Reason = "3 yellow cards" });
        Player(12);
        Rate(PitchPosition.DM, (12, 9));

        var swap = Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps);

        Assert.Equal((5, 12), (swap.Index, swap.InPlayerId));
        Assert.Contains("suspended in Paulistão", swap.Reason);
        Assert.DoesNotContain("treated as out", swap.Reason);
    }

    [Fact]
    public async Task A_ban_whose_scope_cant_be_matched_to_the_next_competition_is_treated_as_out_and_said_so()
    {
        _bans.Add(new PlayerBan { PlayerId = 6, Name = "Player 6", Scope = "National league", MatchesRemaining = 1 });
        Player(12);
        Rate(PitchPosition.DM, (12, 9));

        var swap = Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps);

        Assert.Equal(6, swap.OutPlayerId);
        Assert.Contains("National league", swap.Reason);
        Assert.Contains("treated as out", swap.Reason);
    }

    [Fact]
    public async Task A_starter_at_85_condition_stays_and_one_at_84_goes()
    {
        Player(2, condition: 85);
        Player(3, condition: 84);
        Player(12);
        Rate(PitchPosition.DR, (12, 10));
        Rate(PitchPosition.DC, (12, 10));

        var swap = Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps);

        Assert.Equal((3, 12), (swap.OutPlayerId, swap.InPlayerId));
        Assert.Contains("tired", swap.Reason);
    }

    [Fact]
    public async Task A_tired_starter_only_goes_out_for_a_reserve_rated_at_most_2_below()
    {
        Player(11, condition: 80);
        Player(12);
        Rate(PitchPosition.ST, (12, 7));

        var plan = (await _rotation.SuggestAsync(apply: false)).Value;

        Assert.Empty(plan.Swaps);
        var unfilled = Assert.Single(plan.Unfilled);
        Assert.Equal((11, (int?)null), (unfilled.OutPlayerId, unfilled.InPlayerId));

        Rate(PitchPosition.ST, (12, 8));

        Assert.Equal(12, Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps).InPlayerId);
    }

    [Fact]
    public async Task An_unavailable_starter_goes_out_for_a_weaker_reserve_too()
    {
        Injure(11);
        Player(12);
        Rate(PitchPosition.ST, (12, 3));

        Assert.Equal(12, Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps).InPlayerId);
    }

    [Fact]
    public async Task A_reserve_of_75_sharpness_comes_before_a_better_rated_one_short_of_match_sharpness()
    {
        Injure(11);
        Player(12, sharpness: 74);
        Player(13, sharpness: 75);
        Rate(PitchPosition.ST, (12, 16), (13, 12));

        Assert.Equal(13, Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps).InPlayerId);
    }

    [Fact]
    public async Task Without_a_sharp_reserve_the_best_rated_comes_in_and_his_sharpness_is_flagged()
    {
        Injure(11);
        Player(12, sharpness: 60);
        Player(13, sharpness: 70);
        Rate(PitchPosition.ST, (12, 14), (13, 12));

        var swap = Assert.Single((await _rotation.SuggestAsync(apply: false)).Value.Swaps);

        Assert.Equal(12, swap.InPlayerId);
        Assert.Contains("sharpness", swap.Detail);
    }

    [Fact]
    public async Task Unavailable_or_unrated_reserves_dont_come_in_and_one_reserve_fills_one_slot()
    {
        Injure(10);
        Injure(11);
        Player(12);
        Player(13);
        Injure(13);
        Player(14);
        Rate(PitchPosition.ST, (12, 12), (13, 18));
        Rate(PitchPosition.AML, (12, 12));

        var plan = (await _rotation.SuggestAsync(apply: false)).Value;

        // Player 12 goes to the first slot by index; 14 is rated nowhere and 13 is injured.
        var swap = Assert.Single(plan.Swaps);
        Assert.Equal((9, 12), (swap.Index, swap.InPlayerId));
        Assert.Equal(11, Assert.Single(plan.Unfilled).OutPlayerId);
    }

    [Fact]
    public async Task A_dry_run_plans_the_picks_and_sends_nothing()
    {
        Injure(11);
        Player(12);
        Rate(PitchPosition.ST, (12, 10));

        var plan = (await _rotation.SuggestAsync(apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal(2, plan.Slot);
        var pick = Assert.Single(plan.Edit!.Steps);
        Assert.Equal(("Pick", "slot 10", EditStepStatus.Planned), (pick.Action, pick.Subject, pick.Status));
        await _editor.DidNotReceiveWithAnyArgs().PickAsync(default, default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task Applying_picks_each_reserve_into_his_slot_and_keeps_every_position_and_role()
    {
        Injure(4);
        Player(9, condition: 70);
        Player(12);
        Player(13);
        Rate(PitchPosition.DC, (12, 10));
        Rate(PitchPosition.AMC, (13, 10));

        var plan = (await _rotation.SuggestAsync(apply: true)).Value;

        Assert.True(plan.Applied);
        Assert.Equal([3, 8], plan.Swaps.Select(swap => swap.Index));
        await _saveGuard.Received(1).PrepareWriteAsync(Arg.Any<CancellationToken>());
        await _editor.Received(1).PickAsync(3, 12, Arg.Any<CancellationToken>());
        await _editor.Received(1).PickAsync(8, 13, Arg.Any<CancellationToken>());
        await _editor.DidNotReceiveWithAnyArgs().MoveAsync(default, default, default!);
        await _editor.DidNotReceiveWithAnyArgs().SetRoleAsync(default, default, default!);
        await _editor.DidNotReceiveWithAnyArgs().SwitchSlotAsync(default);
        Assert.Equal(["slot 3", "slot 8"], plan.Edit!.Steps.Select(step => step.Subject));
        Assert.All(plan.Edit.Steps, step => Assert.Equal(EditStepStatus.Done, step.Status));
    }

    [Fact]
    public async Task Rotating_twice_in_a_row_changes_nobody_the_second_time()
    {
        Injure(4);
        Player(9, condition: 70);
        Player(11, condition: 80);
        Player(12);
        Player(13);
        Rate(PitchPosition.DC, (12, 10));
        Rate(PitchPosition.AMC, (13, 10));

        await _rotation.SuggestAsync(apply: true);
        _editor.ClearReceivedCalls();

        var second = (await _rotation.SuggestAsync(apply: true)).Value;

        Assert.Empty(second.Swaps);
        Assert.Equal(11, Assert.Single(second.Unfilled).OutPlayerId);
        Assert.Null(second.Edit);
        await _editor.DidNotReceiveWithAnyArgs().PickAsync(default, default);
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_sends_nothing()
    {
        Injure(11);
        Player(12);
        Rate(PitchPosition.ST, (12, 10));
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _rotation.SuggestAsync(apply: true);

        Assert.Contains("Another save is loaded.", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
        await _editor.DidNotReceiveWithAnyArgs().PickAsync(default, default);
    }

    [Fact]
    public async Task Registration_rules_of_the_next_match_are_flagged_for_the_players_coming_in()
    {
        _nextMatch.EligibilityRules.Add("Only registered players can play");
        Injure(11);
        Player(12);
        Rate(PitchPosition.ST, (12, 10));

        var plan = (await _rotation.SuggestAsync(apply: false)).Value;

        Assert.Contains(plan.Notes, note => note.Contains("Only registered players can play") && note.Contains("Player 12"));
    }

    private void Player(int id, int condition = 95, int sharpness = 80)
    {
        _players[id] = new Player { Id = id, Name = $"Player {id}", Condition = condition, MatchSharpness = sharpness };
    }

    private void Injure(int id)
    {
        _injuries.Add(new PlayerInjury { PlayerId = id, Name = $"Player {id}", Availability = 1 });
    }

    // Every role at the position rates these players so.
    private void Rate(PitchPosition position, params (int Id, int Rating)[] ratings)
    {
        var at = _ratings.TryGetValue(position, out var known) ? known : _ratings[position] = [];

        foreach (var (id, rating) in ratings)
        {
            at[id] = rating;
        }
    }

    private static PitchPosition Position(int index)
    {
        TacticalPosition.TryParse(Codes[index], out var position);
        return position!.Position;
    }

    private RoleFit RoleFit(PitchPosition position)
    {
        var ratings = _ratings.GetValueOrDefault(position) ?? [];

        return new RoleFit
        {
            Position = position,
            InPossession = true,
            Roles = Slots()
                .Where(slot => slot.Position.InPossession!.Position == position)
                .Select(slot => new RoleCandidates
                {
                    Key = slot.Role.InPossession,
                    Role = "Role",
                    Players = ratings.Select(rating => new PlayerRoleRating
                    {
                        PlayerId = rating.Key, Name = $"Player {rating.Key}", Rating = new Rating(rating.Value, rating.Value)
                    }).ToList()
                })
                .ToList()
        };
    }

    private List<EditableSlot> Slots()
    {
        return Codes.Select((code, index) =>
        {
            TacticalPosition.TryParse(code, out var position);
            return new EditableSlot
            {
                Index = index,
                PlayerId = _eleven[index],
                PlayerName = $"Player {_eleven[index]}",
                Position = new ByPhase<TacticalPosition> { InPossession = position, OutOfPossession = position },
                Role = new ByPhase<string> { InPossession = $"Role{index}", OutOfPossession = $"Out{index}" }
            };
        }).ToList();
    }

    private EditableTactic Tactic()
    {
        return new EditableTactic
        {
            ActiveSlot = 2,
            Slots = [1, 2, 3],
            Lineup = Slots(),
            Squad = Enumerable.Range(1, 20).ToDictionary(id => id, id => $"Player {id}")
        };
    }
}
