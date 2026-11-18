using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.SetPieces;

/// <summary>The team's captain and vice-captain.</summary>
internal sealed class BridgeCaptaincyDto
{
    [JsonPropertyName(SetPieceConstants.CaptainId)]
    public int? CaptainId { get; set; }

    [JsonPropertyName(SetPieceConstants.CaptainName)]
    public string? CaptainName { get; set; }

    [JsonPropertyName(SetPieceConstants.ViceCaptainId)]
    public int? ViceCaptainId { get; set; }

    [JsonPropertyName(SetPieceConstants.ViceCaptainName)]
    public string? ViceCaptainName { get; set; }
}
