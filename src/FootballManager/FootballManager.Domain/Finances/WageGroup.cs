namespace FootballManager.Domain.Finances;

/// <summary>The weekly wages of one kind of player (squad status), e.g. "Titular Regular"; "Geral" is the whole squad.</summary>
public class WageGroup
{
    public required string Name { get; set; }
    public int Players { get; set; }

    /// <summary>The group's weekly wages as the game writes them, e.g. "R$810.224".</summary>
    public string? TotalWages { get; set; }

    /// <summary>The same total in the game's unit.</summary>
    public long TotalWagesInGameUnit { get; set; }

    public string? AverageWage { get; set; }
    public string? HighestWage { get; set; }
    public string? LowestWage { get; set; }

    /// <summary>The most the game advises paying one player of this kind.</summary>
    public string? AdvisedMaxWage { get; set; }

    public bool IsOverWages { get; set; }
}
