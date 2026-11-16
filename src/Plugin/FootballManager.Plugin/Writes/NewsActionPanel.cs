using FM.UI;
using FootballManager.Plugin.Data;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Opens a game panel the way a menu entry with IsOpenPanelAction does (make an offer for a player, set a transfer
// target, talk to a player): the UI's OpenNewsActionPanelEvent with the entry's NewsAction id and its
// ActionDataReferences, read from the game as the entry carries them (field=References:@Items[0].ActionData.ActionDataReferences).
internal static class NewsActionPanel
{
    public const string ReferencesField = "References";

    public static async Task<string> OpenAsync(
        DataQuery query, int actionId, IReadOnlyDictionary<string, ValueSource> given, bool send, TimeSpan timeout)
    {
        if (!given.ContainsKey(ReferencesField))
        {
            throw new ArgumentException("Give field=References:@<path to the entry's ActionData>.ActionDataReferences.");
        }

        var (source, contexts) = await DataQueries.ResolveSourceAsync(query, timeout);
        var values = await ValueSources.ResolveAsync(source, contexts, given, timeout);

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var references = TypedValueReader.AsList(values[ReferencesField])
                ?? throw new ArgumentException("References isn't a list.");
            var list = new Il2CppSystem.Collections.Generic.List<TypedValue>();

            for (var i = 0; i < references.Count; i++)
            {
                list.Add(references[i]);
            }

            if (send)
            {
                Subsystems.Get<EventSubsystem>().SendEvent(new OpenNewsActionPanelEvent
                {
                    ActionId = actionId,
                    DynamicCardId = -1,
                    ActionDataReferences = list,
                    PortalSourceId = 0
                });
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["event"] = "FM.UI.OpenNewsActionPanelEvent",
                ["actionId"] = actionId,
                ["references"] = list.Count,
                ["sent"] = send
            });
        });
    }
}
