namespace FootballManager.Infrastructure.Dtos.Saves;

/// <summary>The plugin's /session: which save the game has open, by its file name.</summary>
internal sealed class BridgeSessionDto
{
    public bool Loaded { get; set; }
    public bool Loading { get; set; }
    public string? LoadedSave { get; set; }
    public bool IsInvestigationSave { get; set; }
}
