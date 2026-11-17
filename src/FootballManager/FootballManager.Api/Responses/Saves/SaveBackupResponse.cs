using FootballManager.Domain.Saves;

namespace FootballManager.Api.Responses.Saves;

public class SaveBackupResponse
{
    public string Name { get; set; } = "";

    /// <summary>BeforeWrite (rotated: each club's two newest kept), BeforeRestore or Other (never removed).</summary>
    public SaveBackupKind Kind { get; set; }

    public string? Club { get; set; }
    public DateTime? CopiedAt { get; set; }

    /// <summary>The game's date in the save when it was copied.</summary>
    public DateOnly? GameDate { get; set; }
}
