namespace FootballManager.Application.Interfaces;

/// <summary>The few file operations the save guard needs, on the machine running the game.</summary>
public interface ISaveFiles
{
    bool Exists(string path);

    /// <summary>The names of the files directly in <paramref name="folder"/>; none when it doesn't exist.</summary>
    IReadOnlyList<string> ListFileNames(string folder);

    /// <summary>The file's text, or null when it doesn't exist.</summary>
    string? ReadText(string path);

    /// <summary>Copies a file, creating the destination's folder; never overwrites.</summary>
    void Copy(string source, string destination);

    void Delete(string path);
}
