using System.Text.Json.Serialization;
using FootballManager.Domain.Transfers;

namespace FootballManager.Api.Responses.Transfers;

public class PlayerExitResponse
{
    /// <summary>Written, NotWritten (also a dry run) or Unknown: then check with get_squad before sending it again.</summary>
    public PlayerExitWrite Applied { get; set; }

    /// <summary>Whether the read back shows the change; null when it can't tell (see notes). Always sent, null included.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Confirmed { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerExitAction Action { get; set; }

    /// <summary>The menu entry picked, as the game labels each level.</summary>
    public string? Choice { get; set; }

    /// <summary>Every entry the game's menu offers for the action.</summary>
    public List<string>? Choices { get; set; }

    /// <summary>What the game asks to confirm before running the entry.</summary>
    public string? Confirmation { get; set; }

    /// <summary>Where the player stands: before the change on a dry run, after it once applied.</summary>
    public required PlayerExitStatusResponse Status { get; set; }

    public List<string>? Notes { get; set; }
}
