namespace FootballManager.Domain.Staff;

/// <summary>A member of the first team's staff: who he is, his job, how he coaches, what he's good at and his contract.</summary>
public class StaffMember
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }

    /// <summary>His job, as the game names it, e.g. "Treinador".</summary>
    public string? Job { get; set; }

    /// <summary>How he coaches, e.g. "Tático".</summary>
    public string? CoachingStyle { get; set; }

    public List<StaffAttribute> CoachingAttributes { get; set; } = [];

    /// <summary>Empty for staff without medical attributes, e.g. the manager.</summary>
    public List<StaffAttribute> MedicalAttributes { get; set; } = [];

    /// <summary>The weekly wage as the game writes it, e.g. "R$13 mil p/s".</summary>
    public string? Wage { get; set; }

    /// <summary>yyyy-MM-dd; null for a contract without an end.</summary>
    public string? ContractEnd { get; set; }
}
