namespace FootballManager.Domain.Registration;

/// <summary>A registration change in one competition, as planned or sent.</summary>
public class RegistrationChange
{
    public bool Applied { get; set; }

    /// <summary>After an apply: whether the registration read back has every player where asked; null when it couldn't be read.</summary>
    public bool? Confirmed { get; set; }

    public required string Competition { get; set; }
    public List<RegistrationPlayer> Register { get; set; } = [];
    public List<RegistrationPlayer> Unregister { get; set; } = [];
    public List<RuleCheck> Rules { get; set; } = [];

    /// <summary>Each player's write, by id, then the confirm's (the registration screen's Confirm button).</summary>
    public Dictionary<int, RegistrationWrite> Writes { get; set; } = [];

    public RegistrationWrite Confirm { get; set; }

    public List<string> Warnings { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
