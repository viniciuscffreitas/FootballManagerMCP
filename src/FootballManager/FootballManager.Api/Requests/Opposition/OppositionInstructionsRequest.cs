using FootballManager.Domain.Opposition;

namespace FootballManager.Api.Requests.Opposition;

public class OppositionInstructionsRequest
{
    /// <summary>Required: left out, it is refused rather than read as the enum's first value.</summary>
    public OppositionInstructionsAction? Action { get; set; }

    /// <summary>An opponent player of the lineup the game expects, by FM id or name.</summary>
    public string? Player { get; set; }

    /// <summary>none, never or always.</summary>
    public string? Marking { get; set; }

    /// <summary>none, never or always.</summary>
    public string? Pressing { get; set; }

    /// <summary>none, normal, soft or hard.</summary>
    public string? Tackling { get; set; }

    /// <summary>none, left, right or weaker.</summary>
    public string? ShowOntoFoot { get; set; }

    /// <summary>The instructions given, by instruction.</summary>
    public Dictionary<OppositionInstruction, string> Settings()
    {
        var settings = new Dictionary<OppositionInstruction, string>();
        Add(settings, OppositionInstruction.Marking, Marking);
        Add(settings, OppositionInstruction.Pressing, Pressing);
        Add(settings, OppositionInstruction.Tackling, Tackling);
        Add(settings, OppositionInstruction.ShowOntoFoot, ShowOntoFoot);
        return settings;
    }

    private static void Add(Dictionary<OppositionInstruction, string> settings, OppositionInstruction instruction, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            settings[instruction] = value;
        }
    }
}
