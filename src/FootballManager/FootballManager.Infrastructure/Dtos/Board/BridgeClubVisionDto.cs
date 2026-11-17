using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Board;

/// <summary>The club's vision and board requests, keyed by the game's property paths.</summary>
internal sealed class BridgeClubVisionDto
{
    [JsonPropertyName(BoardConstants.ThisYearName)]
    public string? ThisYearName { get; set; }

    [JsonPropertyName(BoardConstants.ThisYearYear)]
    public string? ThisYearYear { get; set; }

    [JsonPropertyName(BoardConstants.ThisYearProgress)]
    public int? ThisYearProgress { get; set; }

    [JsonPropertyName(BoardConstants.ThisYearStatus)]
    public int? ThisYearStatus { get; set; }

    [JsonPropertyName(BoardConstants.FinalName)]
    public string? FinalName { get; set; }

    [JsonPropertyName(BoardConstants.FinalYear)]
    public string? FinalYear { get; set; }

    [JsonPropertyName(BoardConstants.FinalProgress)]
    public int? FinalProgress { get; set; }

    [JsonPropertyName(BoardConstants.FinalStatus)]
    public int? FinalStatus { get; set; }

    [JsonPropertyName(BoardConstants.BoardRequests)]
    public int? BoardRequests { get; set; }

    [JsonPropertyName(BoardConstants.ActiveBoardRequests)]
    public int? ActiveBoardRequests { get; set; }

    [JsonPropertyName(BoardConstants.SupporterCulture)]
    public string? SupporterCulture { get; set; }

    [JsonPropertyName(BoardConstants.BoardCulture)]
    public string? BoardCulture { get; set; }
}
