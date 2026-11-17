namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>
/// A finance projection: the amount at the end of this season and the next two. Its monthly graph is left out: the game
/// dates it from 1900.
/// </summary>
internal sealed class BridgeProjectionDto
{
    public BridgeProjectionPointDto? ProjectionEndOfSeasonOne { get; set; }
    public BridgeProjectionPointDto? ProjectionEndOfSeasonTwo { get; set; }
    public BridgeProjectionPointDto? ProjectionEndOfSeasonThree { get; set; }
}
