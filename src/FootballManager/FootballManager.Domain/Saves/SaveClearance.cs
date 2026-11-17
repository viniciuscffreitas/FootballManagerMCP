namespace FootballManager.Domain.Saves;

/// <summary>What the save guard checked before letting a write through, and the copy it made.</summary>
public class SaveClearance
{
    public required string SaveName { get; set; }

    /// <summary>The test save is cleared without a copy or a version check.</summary>
    public bool IsTestSave { get; set; }

    public string? Club { get; set; }
    public DateOnly? GameDate { get; set; }
    public string? GameVersion { get; set; }

    /// <summary>The copy made of the real save before the write; null for the test save.</summary>
    public string? Backup { get; set; }

    /// <summary>Older copies of the guard's own removed by the rotation.</summary>
    public List<string> RemovedBackups { get; set; } = [];

    /// <summary>What the reader should know, e.g. an old copy the rotation couldn't remove.</summary>
    public List<string> Notes { get; set; } = [];
}
