using System.Text.Json;
using FootballManager.Domain.Players;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Mappings;

// A player's season numbers out of the paths read beside his other values. Null when they weren't asked for.
internal static class PerformanceMapper
{
    private const string ValueField = "PropertyValue";

    public static PlayerPerformance? ToPerformance(Dictionary<string, JsonElement> values)
    {
        if (!values.ContainsKey(PerformanceConstants.Appearances))
        {
            return null;
        }

        var performance = new PlayerPerformance();

        foreach (var (path, set) in PerformanceConstants.Stats)
        {
            set(performance, values.TryGetValue(path, out var value) ? Number(value) : null);
        }

        return performance;
    }

    public static string? League(Dictionary<string, JsonElement> values)
    {
        return values.TryGetValue(PerformanceConstants.League, out var league) && league.ValueKind == JsonValueKind.String
            ? Blank(league.GetString())
            : null;
    }

    // A plain number, or the small record the game keeps most season numbers in.
    private static double? Number(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.Object when value.TryGetProperty(ValueField, out var inner) && inner.ValueKind == JsonValueKind.Number
                => inner.GetDouble(),
            _ => null
        };
    }

    private static string? Blank(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
