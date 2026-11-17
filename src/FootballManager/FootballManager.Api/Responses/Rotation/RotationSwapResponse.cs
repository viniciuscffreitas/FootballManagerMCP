namespace FootballManager.Api.Responses.Rotation;

public class RotationSwapResponse
{
    /// <summary>The slot's index in the lineup, 0 to 10, as get_tactics lists it.</summary>
    public int Index { get; set; }

    public string? Position { get; set; }
    public string? Role { get; set; }
    public int OutPlayerId { get; set; }
    public required string OutName { get; set; }
    public string? OutRating { get; set; }
    public required string Reason { get; set; }
    public int? InPlayerId { get; set; }
    public string? InName { get; set; }
    public string? InRating { get; set; }
    public int? InCondition { get; set; }
    public int? InMatchSharpness { get; set; }
    public string? Detail { get; set; }
}
