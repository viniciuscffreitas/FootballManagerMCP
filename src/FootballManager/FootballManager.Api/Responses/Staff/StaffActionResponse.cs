using System.Text.Json.Serialization;
using FootballManager.Domain.Staff;

namespace FootballManager.Api.Responses.Staff;

public class StaffActionResponse
{
    public bool Applied { get; set; }
    public StaffMenuAction Action { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public List<StaffMenuEntryResponse> Menu { get; set; } = [];
    public string? Choice { get; set; }
    public string? Confirmation { get; set; }
    public bool ConfirmationIsWarning { get; set; }
    /// <summary>Whether the menu still offers the action after the send; null when the read back was inconclusive.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? StillOffered { get; set; }
    public List<string> Notes { get; set; } = [];
}
