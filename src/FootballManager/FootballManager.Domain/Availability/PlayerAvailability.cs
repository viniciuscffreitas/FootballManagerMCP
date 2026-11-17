namespace FootballManager.Domain.Availability;

/// <summary>A first-team player's fitness and whether he can be picked for the next match.</summary>
public class PlayerAvailability
{
    /// <summary>FM's unique id for the person.</summary>
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Positions as FM abbreviates them, e.g. "D (C)".</summary>
    public string? Positions { get; set; }
    public string? BestRole { get; set; }

    /// <summary>Physical condition in percent; null while the game hides it (injured).</summary>
    public int? Condition { get; set; }

    /// <summary>Match sharpness in percent.</summary>
    public int? MatchSharpness { get; set; }

    /// <summary>Average match rating this season; null without appearances.</summary>
    public double? AverageRating { get; set; }

    /// <summary>Why he can't be picked; empty when he can.</summary>
    public List<Absence> Absences { get; set; } = [];

    public bool Available => Absences.Count == 0;
}
