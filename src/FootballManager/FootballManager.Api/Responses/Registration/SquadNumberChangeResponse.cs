using System.Text.Json.Serialization;
using FootballManager.Domain.Registration;

namespace FootballManager.Api.Responses.Registration;

public class SquadNumberChangeResponse
{
    public bool Applied { get; set; }

    /// <summary>Whether the read back matches the request; null for a dry run or when it couldn't be read.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Confirmed { get; set; }

    public bool AskAssistant { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int? From { get; set; }
    public int? To { get; set; }
    public int? NeedingNumbersBefore { get; set; }
    public int? NeedingNumbersAfter { get; set; }

    /// <summary>Written, NotWritten (also a dry run) or Unknown: then check with get_squad before sending again.</summary>
    public RegistrationWrite Write { get; set; }

    public List<string> Warnings { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
