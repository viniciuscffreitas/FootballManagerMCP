using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Contracts;

public class ContractChangeResponse
{
    public bool Applied { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerMenuAction Action { get; set; }
    public required string Choice { get; set; }
    public string? Confirmation { get; set; }
    public bool ConfirmationIsWarning { get; set; }
    public string? AgreedPlayingTime { get; set; }
    public string? AgreedPlayingTimeAfter { get; set; }
    public List<string> Choices { get; set; } = [];
    public string? DialogAnswered { get; set; }
    public bool? StillInSquad { get; set; }
    public List<string> Notes { get; set; } = [];
}
