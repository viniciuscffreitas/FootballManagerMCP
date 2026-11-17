namespace FootballManager.Infrastructure.Dtos.Staff;

/// <summary>One item of a staff member's AttributesCoaching or AttributesMedical list.</summary>
internal sealed class BridgeStaffAttributeDto
{
    public string? Name { get; set; }

    /// <summary>1 to 20.</summary>
    public int? PropertyValue { get; set; }

    /// <summary>The game's word for the value, e.g. "Bom".</summary>
    public string? DisplayString { get; set; }
}
