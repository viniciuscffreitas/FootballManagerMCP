using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Board;

/// <summary>The budget adjustment panel's record; every value is missing while the panel is closed.</summary>
internal sealed class BridgeBudgetPanelDto
{
    [JsonPropertyName(BoardEditingConstants.SliderValue)]
    public float? SliderValue { get; set; }

    [JsonPropertyName(BoardEditingConstants.TransferValue)]
    public string? TransferBudget { get; set; }

    [JsonPropertyName(BoardEditingConstants.WageValue)]
    public string? WageBudget { get; set; }

    [JsonPropertyName(BoardEditingConstants.CanEdit)]
    public bool? CanEdit { get; set; }

    [JsonPropertyName(BoardEditingConstants.CanEditUp)]
    public bool? CanEditUp { get; set; }

    [JsonPropertyName(BoardEditingConstants.CanEditDown)]
    public bool? CanEditDown { get; set; }

    [JsonPropertyName(BoardEditingConstants.BoardText)]
    public string? Board { get; set; }
}
