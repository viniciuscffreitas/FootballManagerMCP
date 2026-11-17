namespace FootballManager.Api.Responses.Opposition;

public class ReportPointResponse
{
    public required string Topic { get; set; }
    public string? Summary { get; set; }
    public string? PlayerName { get; set; }
    public string? TimeFrame { get; set; }
}
