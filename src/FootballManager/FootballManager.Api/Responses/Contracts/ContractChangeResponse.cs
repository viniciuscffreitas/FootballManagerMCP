using System.Text.Json.Serialization;
using FootballManager.Domain.Contracts;
using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Contracts;

public class ContractChangeResponse
{
    public AppliedState Applied { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerMenuAction Action { get; set; }
    public required string Choice { get; set; }
    public string? Confirmation { get; set; }
    public bool ConfirmationIsWarning { get; set; }
    public string? AgreedPlayingTime { get; set; }
    public string? AgreedPlayingTimeAfter { get; set; }
    public List<string> Choices { get; set; } = [];
    public MenuWrite Menu { get; set; }
    public DialogState? Dialog { get; set; }
    public string? DialogText { get; set; }
    public bool? StillInSquad { get; set; }

    /// <summary>Null when the read back can't tell: serialized as null rather than left out.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Confirmed { get; set; }
    public List<string> Notes { get; set; } = [];
}
