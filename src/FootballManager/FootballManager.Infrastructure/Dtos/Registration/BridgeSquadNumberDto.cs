using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Registration;

/// <summary>A player on the squad numbers screen's edit session, read with him passed as Person.</summary>
internal sealed class BridgeSquadNumberDto
{
    [JsonPropertyName(RegistrationConstants.CanChangeNumber)]
    public bool? CanChange { get; set; }

    [JsonPropertyName(RegistrationConstants.SuggestedNumber)]
    public int? Suggested { get; set; }

    // Each number from -1 (none) to 99: RowData.PropertyValue the number, DisplayValue "10. Neymar" when someone wears
    // it, Disabled then.
    [JsonPropertyName(RegistrationConstants.NumberDropdown)]
    public List<BridgeSquadNumberOptionDto>? Options { get; set; }
}
