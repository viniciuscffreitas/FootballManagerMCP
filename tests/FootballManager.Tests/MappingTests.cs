using FootballManager.Api.Responses.Opposition;
using FootballManager.Domain.Opposition;
using FootballManager.Domain.Tactics;
using Mapster;

namespace FootballManager.Tests;

public class MappingTests
{
    // Every mapping is a Mapster config: compiling them all catches a renamed or retyped field before a tool call does.
    [Fact]
    public void Api_and_infrastructure_mappings_compile()
    {
        var config = new TypeAdapterConfig();
        new Infrastructure.Mappings.MappingConfig().Register(config);
        new Api.Mappings.MappingConfig().Register(config);

        config.Compile();
    }

    [Fact]
    public void An_opposition_instructions_change_gives_positions_as_codes()
    {
        var config = new TypeAdapterConfig();
        new Api.Mappings.MappingConfig().Register(config);
        var change = new OppositionInstructionsChange
        {
            Action = OppositionInstructionsAction.Set,
            Current = new OppositionInstructions
            {
                TeamId = 324,
                TeamName = "GRE",
                Players = [new OppositionPlayerInstructions { PlayerId = 101, Position = new TacticalPosition(PitchPosition.DC, PositionSide.Left), Marking = "always" }]
            },
            Plan = [new OppositionInstructionWrite { PlayerId = 101, Instruction = OppositionInstruction.Marking, From = "none", To = "always" }]
        };

        var response = change.Adapt<OppositionInstructionsChangeResponse>(config);

        Assert.Equal(("DCL", "always"), (response.Current.Players[0].Position, response.Current.Players[0].Marking));
        Assert.Equal(OppositionInstruction.Marking, response.Plan[0].Instruction);
    }
}
