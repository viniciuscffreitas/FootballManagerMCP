using FootballManager.Application.Interfaces;
using FootballManager.Application.Services;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using NSubstitute;

namespace FootballManager.Tests;

// The eleven by id alone, in the order of the slots' index as get_tactics lists them.
public class LineupByIdTests
{
    private static readonly string[] Codes = ["GK", "DR", "DCR", "DCL", "DL", "DMR", "DML", "AMR", "AMC", "AML", "ST"];

    private readonly ITacticEditorGameClient _game = Substitute.For<ITacticEditorGameClient>();
    private readonly TacticEditingService _editing;

    public LineupByIdTests()
    {
        _game.GetOptionsAsync(Arg.Any<CancellationToken>()).Returns(new TacticOptions
        {
            ActiveSlot = 1,
            Roles = new ByPhase<List<string>>
            {
                InPossession = [.. Codes.Select((_, index) => $"Role{index}")],
                OutOfPossession = [.. Codes.Select((_, index) => $"Out{index}")]
            }
        });
        _game.GetTacticAsync(Arg.Any<CancellationToken>()).Returns(_ => Tactic());
        _game.GetRolesAsync(Arg.Any<int>(), Arg.Any<PitchPosition>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => Tactic().Lineup
                .Where(slot => slot.Position.InPossession!.Position == call.ArgAt<PitchPosition>(1))
                .Select(slot => new RoleRating { Key = slot.Role.For(call.ArgAt<bool>(2)), Role = "Role" })
                .ToList());
        _editing = new TacticEditingService(_game, Substitute.For<ISaveGuard>());
    }

    [Fact]
    public async Task A_newcomer_listed_by_id_takes_the_position_of_the_slot_he_is_listed_at()
    {
        // Player 12 comes in at index 3, for player 4.
        var lineup = Enumerable.Range(1, 11).Select(id => new LineupChange { PlayerId = id == 4 ? 12 : id }).ToList();

        var result = await _editing.SetTacticAsync(new TacticChanges { Slot = 1, Lineup = lineup }, apply: false);

        Assert.True(result.IsSuccess, string.Join(" ", result.Errors.Select(error => error.Message)));
        var pick = Assert.Single(result.Value.Steps, step => step.Action == "Pick");
        Assert.Equal(("slot 3", "Player 4", "Player 12"), (pick.Subject, pick.From, pick.To));
        Assert.DoesNotContain(result.Value.Steps, step => step.Action is "Move" or "Role");
    }

    private static EditableTactic Tactic()
    {
        return new EditableTactic
        {
            ActiveSlot = 1,
            Slots = [1],
            Lineup = Codes.Select((code, index) =>
            {
                TacticalPosition.TryParse(code, out var position);
                return new EditableSlot
                {
                    Index = index, PlayerId = index + 1, PlayerName = $"Player {index + 1}",
                    Position = new ByPhase<TacticalPosition> { InPossession = position, OutOfPossession = position },
                    Role = new ByPhase<string> { InPossession = $"Role{index}", OutOfPossession = $"Out{index}" }
                };
            }).ToList(),
            Squad = Enumerable.Range(1, 20).ToDictionary(id => id, id => $"Player {id}")
        };
    }
}
