using System.Text.Json.Serialization;
using FootballManager.Domain.Registration;

namespace FootballManager.Api.Responses.Registration;

public class RegistrationChangeResponse
{
    public bool Applied { get; set; }

    /// <summary>Whether the read back has every player where asked; null for a dry run or when it couldn't be read.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Confirmed { get; set; }

    public required string Competition { get; set; }
    public List<RegistrationPlayerResponse> Register { get; set; } = [];
    public List<RegistrationPlayerResponse> Unregister { get; set; } = [];
    public List<RuleCheckResponse> Rules { get; set; } = [];

    /// <summary>Each player's write by id: Written, NotWritten (also a dry run) or Unknown (check before sending again).</summary>
    public Dictionary<int, RegistrationWrite> Writes { get; set; } = [];

    public RegistrationWrite Confirm { get; set; }
    public List<string> Warnings { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
