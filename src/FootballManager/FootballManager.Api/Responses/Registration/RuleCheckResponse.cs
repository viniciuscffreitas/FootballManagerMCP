using System.Text.Json.Serialization;

namespace FootballManager.Api.Responses.Registration;

public class RuleCheckResponse
{
    public required string Rule { get; set; }
    public int? Before { get; set; }
    public int? After { get; set; }
    public int? Limit { get; set; }

    /// <summary>Null when the plan can't tell: the game checks the rule on confirming.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Broken { get; set; }
}
