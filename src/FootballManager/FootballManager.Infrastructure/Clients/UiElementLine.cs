namespace FootballManager.Infrastructure.Clients;

/// <summary>
/// An element of the game's UI as the plugin's /ui lists it, one per line: its path (what /click takes), its type
/// with its name ("SIButton#buttons-controls-pc-default"), its classes and its text, then its bounds.
/// </summary>
internal sealed record UiElementLine(string Path, string Type, IReadOnlyList<string> Classes, string? Text)
{
    // e.g. `  0/3/5/1 SIButton#name .suggestion-button .other "Seu treino está excelente." @120,300 400x40`
    public static UiElementLine? Parse(string line)
    {
        var trimmed = line.Trim();
        var bounds = trimmed.LastIndexOf(" @", StringComparison.Ordinal);

        if (bounds < 0)
        {
            return null;
        }

        var head = trimmed[..bounds];
        string? text = null;
        var quote = head.IndexOf(" \"", StringComparison.Ordinal);

        if (quote >= 0)
        {
            var end = head.LastIndexOf('"');
            text = end > quote + 1 ? head[(quote + 2)..end] : null;
            head = head[..quote];
        }

        var tokens = head.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // The documents' own header lines ("[0] UIDocument on ...") aren't elements.
        if (tokens.Length < 2 || !char.IsDigit(tokens[0][0]))
        {
            return null;
        }

        return new UiElementLine(tokens[0], tokens[1],
            tokens.Skip(2).Where(token => token.StartsWith('.')).Select(token => token[1..]).ToList(), text);
    }

    /// <summary>Whether this element is <paramref name="ancestor"/> or lies under it.</summary>
    public bool IsWithin(string ancestor)
    {
        return Path == ancestor || Path.StartsWith(ancestor + "/", StringComparison.Ordinal);
    }

    public bool IsButton => Type == ConversationButton || Type.StartsWith(ConversationButton + "#", StringComparison.Ordinal);

    private const string ConversationButton = Constants.ConversationConstants.Button;
}
