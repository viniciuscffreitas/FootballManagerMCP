namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>The screen's rows: the expected lineup's players, and the positions (rows without a player, Index -1).</summary>
internal sealed class BridgeOppositionRowsDto
{
    public List<BridgeOppositionRowDto>? OppositionPredictedLineupInstructions { get; set; }
    public List<BridgeOppositionRowDto>? OppositionAllPositionsInstructions { get; set; }
}
