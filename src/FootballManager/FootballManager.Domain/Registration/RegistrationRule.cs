namespace FootballManager.Domain.Registration;

/// <summary>A registration rule as the game states it: its type, its words, the squad's value against it and whether it's met.</summary>
public class RegistrationRule
{
    /// <summary>The game's rule type: 9 is the maximum squad size, 165 the minimum, 87 the minimum of goalkeepers; null when it didn't say.</summary>
    public int? Type { get; set; }

    public string? Title { get; set; }

    /// <summary>The rule in the game's words, e.g. "Tamanho máximo do elenco de 50 jogadores".</summary>
    public string? Text { get; set; }

    /// <summary>How many players count for it now.</summary>
    public int? Value { get; set; }

    /// <summary>The game's value against the rule, e.g. "47/50 Máx.".</summary>
    public string? Display { get; set; }

    public bool Met { get; set; }
}
