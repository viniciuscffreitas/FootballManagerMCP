namespace FootballManager.Infrastructure.Dtos.Registration;

/// <summary>A number in the squad numbers dropdown: its value and label in RowData, Disabled when someone wears it.</summary>
internal sealed class BridgeSquadNumberOptionDto
{
    public BridgeOptionDto? RowData { get; set; }
    public bool? Disabled { get; set; }
}
