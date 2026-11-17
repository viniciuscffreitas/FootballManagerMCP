using FM.Game;
using FM.GamePlugin;
using FootballManager.Plugin.Data;
using SI.Core;
using SI.Interop;

namespace FootballManager.Plugin.Writes;

// The context menu runs an entry through PluginContextMenuContributor.PerformAction, not through an event: for entries
// that open a panel (Event 0, such as praising a player's training, which opens the quick chat card) the native
// EventFmxDoAction and OpenNewsActionPanelEvent open nothing. /contextaction calls PerformAction the same way.
internal static class ContextAction
{
    public const string DataField = "Data";

    public static async Task<string> PerformAsync(
        DataQuery query, uint actionId, uint eventId, bool flag, IReadOnlyDictionary<string, ValueSource> given, bool send, TimeSpan timeout)
    {
        if (!given.ContainsKey(DataField))
        {
            throw new ArgumentException("Give field=Data:@Items[0].ActionData, the entry's ActionData.");
        }

        var (source, contexts) = await DataQueries.ResolveSourceAsync(query, timeout);
        var values = await ValueSources.ResolveAsync(source, contexts, given, timeout);

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var target = TypedValueReader.AsReference(source)
                ?? throw new ArgumentException("The request's base isn't a record: read from base=Action.");
            var data = TypedValue.CreateCopyFrom(values[DataField]);

            if (send)
            {
                var contributor = new PluginContextMenuContributor(new Il2CppSystem.Collections.Generic.List<FM.UI.ActionGroupings>());
                contributor.PerformAction((ActionID)actionId, eventId, target, data, flag);
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["call"] = "PluginContextMenuContributor.PerformAction",
                ["actionId"] = actionId,
                ["eventId"] = eventId,
                ["flag"] = flag,
                ["sent"] = send
            });
        });
    }

    // Harmony prefix on PerformAction: records what a real menu click passes.
    public static void OnPerformAction(object[] __args)
    {
        if (!EventRecorder.IsRecording)
        {
            return;
        }

        try
        {
            EventRecorder.Record(InteropRecorder.UiType, "PluginContextMenuContributor.PerformAction", recorded =>
            {
                recorded.AddNote("ActionID", Convert.ToUInt32(__args[0]));
                recorded.AddNote("EventID", __args[1]);
                recorded.AddReference("Target", __args[2] as InteropReference);
                recorded.AddValue("Data", __args[3] as TypedValue);
                recorded.AddNote("Flag", __args[4]);
            });
        }
        catch (Exception)
        {
        }
    }
}
