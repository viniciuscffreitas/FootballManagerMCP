namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>
/// What a report chart looks at (e.g. "Criatividade - Meio-campistas"), over which span, the game's verdict, and the
/// record it's about (a player, or the team itself).
/// </summary>
internal sealed class BridgeObservationDto
{
    public string? GraphType { get; set; }
    public string? TimeFrame { get; set; }
    public string? PerformanceSummary { get; set; }
    public BridgeReferenceDto? SubjectRecordId { get; set; }
}
