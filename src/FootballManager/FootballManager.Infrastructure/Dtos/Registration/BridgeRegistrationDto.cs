using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Registration;

/// <summary>A competition's registration edit session, keyed by the game's property paths.</summary>
internal sealed class BridgeRegistrationDto
{
    [JsonPropertyName(RegistrationConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(RegistrationConstants.CompetitionName)]
    public string? Competition { get; set; }

    [JsonPropertyName(RegistrationConstants.CompetitionId)]
    public int? CompetitionId { get; set; }

    [JsonPropertyName(RegistrationConstants.WindowOpen)]
    public bool? WindowOpen { get; set; }

    /// <summary>e.g. "Faltam 98 dias", "Reabre em 77 dias".</summary>
    [JsonPropertyName(RegistrationConstants.Window)]
    public string? Window { get; set; }

    /// <summary>yyyy-MM-dd.</summary>
    [JsonPropertyName(RegistrationConstants.Deadline)]
    public string? Deadline { get; set; }

    [JsonPropertyName(RegistrationConstants.MaxSquadSize)]
    public int? MaxSquadSize { get; set; }

    [JsonPropertyName(RegistrationConstants.MeetsRules)]
    public bool? MeetsRules { get; set; }

    [JsonPropertyName(RegistrationConstants.RulesSummary)]
    public string? RulesSummary { get; set; }

    [JsonPropertyName(RegistrationConstants.Notes)]
    public string? Notes { get; set; }

    [JsonPropertyName(RegistrationConstants.Rules)]
    public List<BridgeRegistrationRuleDto>? Rules { get; set; }

    // A player's tick, read with him passed as Person.
    [JsonPropertyName(RegistrationConstants.CanChange)]
    public bool? CanChange { get; set; }
}
