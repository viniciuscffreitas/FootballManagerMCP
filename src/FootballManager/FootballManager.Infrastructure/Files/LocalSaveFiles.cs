using FootballManager.Application.Interfaces;

namespace FootballManager.Infrastructure.Files;

// The API runs on the machine with the game, so the saves and their copies are local files.
internal sealed class LocalSaveFiles : ISaveFiles
{
    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public IReadOnlyList<string> ListFileNames(string folder)
    {
        return Directory.Exists(folder)
            ? Directory.GetFiles(folder).Select(path => Path.GetFileName(path)).ToList()
            : [];
    }

    public string? ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public void Copy(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, overwrite: false);
    }

    public void Replace(string source, string destination)
    {
        File.Copy(source, destination, overwrite: true);
    }

    public void WriteText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    public void Delete(string path)
    {
        File.Delete(path);
    }
}
