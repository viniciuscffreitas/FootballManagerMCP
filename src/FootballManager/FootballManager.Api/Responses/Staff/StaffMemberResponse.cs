namespace FootballManager.Api.Responses.Staff;

public class StaffMemberResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Job { get; set; }
    public string? CoachingStyle { get; set; }
    public List<StaffAttributeResponse>? CoachingAttributes { get; set; }
    public List<StaffAttributeResponse>? MedicalAttributes { get; set; }

    /// <summary>The weekly wage as the game writes it, e.g. "R$13 mil p/s".</summary>
    public string? Wage { get; set; }

    /// <summary>yyyy-MM-dd; null for a contract without an end.</summary>
    public string? ContractEnd { get; set; }
}
