namespace FootballManager.Api.Responses.Opposition;

public class ExpectedPlayerResponse
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }

    /// <summary>FM's position code with the side for central slots, e.g. "DCL", as get_tactics gives it.</summary>
    public string? Position { get; set; }

    public string? RoleKey { get; set; }
    public string? Duty { get; set; }
}
