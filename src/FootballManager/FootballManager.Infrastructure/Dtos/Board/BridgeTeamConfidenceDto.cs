using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Board;

/// <summary>The team's confidence record, keyed by the game's property paths.</summary>
internal sealed class BridgeTeamConfidenceDto
{
    [JsonPropertyName(BoardConstants.BoardGrade)]
    public int? BoardGrade { get; set; }

    [JsonPropertyName(BoardConstants.BoardSummary)]
    public string? BoardSummary { get; set; }

    [JsonPropertyName(BoardConstants.BoardMatches)]
    public string? BoardMatches { get; set; }

    [JsonPropertyName(BoardConstants.BoardSquadSupport)]
    public string? BoardSquadSupport { get; set; }

    [JsonPropertyName(BoardConstants.BoardTactics)]
    public string? BoardTactics { get; set; }

    [JsonPropertyName(BoardConstants.BoardVision)]
    public string? BoardVision { get; set; }

    [JsonPropertyName(BoardConstants.BoardTransfers)]
    public string? BoardTransfers { get; set; }

    [JsonPropertyName(BoardConstants.BoardHighlights)]
    public List<string>? BoardHighlights { get; set; }

    [JsonPropertyName(BoardConstants.BoardCriticism)]
    public List<string>? BoardCriticism { get; set; }

    [JsonPropertyName(BoardConstants.SupporterGrade)]
    public int? SupporterGrade { get; set; }

    [JsonPropertyName(BoardConstants.SupporterSummary)]
    public string? SupporterSummary { get; set; }

    [JsonPropertyName(BoardConstants.SupporterMatches)]
    public string? SupporterMatches { get; set; }

    [JsonPropertyName(BoardConstants.SupporterVision)]
    public string? SupporterVision { get; set; }
}
