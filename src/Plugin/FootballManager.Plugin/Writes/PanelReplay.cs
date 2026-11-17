using FootballManager.Plugin.Data;
using SI.Bindable;
using SI.Bindable.Events;
using SI.Core;
using PanelAsset = UnityEngine.AddressableAssets.AssetReferenceT<SI.Bindable.PanelID>;

namespace FootballManager.Plugin.Writes;

// Keeps the panels the UI opened while recording (a player's quick chat, a profile), so the plugin can open the same
// panel again without a click: OpenPanelEvent names the panel by its asset and passes it arguments, each a binding key
// and path the panel binds its data from. /openpanel resends one, with the arguments given or as recorded.
internal static class PanelReplay
{
    private const int Capacity = 20;

    private sealed record Argument(string? Name, ulong Key, string? Path, int Method);

    private sealed record Captured(System.DateTime Utc, PanelAsset Panel, string? Guid, List<Argument> Arguments, string? Anchor, int Rebind);

    private static readonly List<Captured> Panels = [];

    // Called from the recorder's UI hook, on the main thread: copies the event, which the game pools and reuses.
    public static void Capture(OpenPanelEvent evt, Action<string, object?> note)
    {
        var arguments = new List<Argument>();
        var list = evt.Arguments;

        for (var i = 0; list is not null && i < list.Count; i++)
        {
            var argument = list[i];
            arguments.Add(new Argument(argument.name, argument.key.m_key, argument.path, (int)argument.method));
        }

        var panel = evt.PanelID;
        var guid = panel?.AssetGUID;

        if (panel is not null)
        {
            Panels.Add(new Captured(System.DateTime.UtcNow, panel, guid, arguments, evt.Anchor, (int)evt.RebindMethod));

            if (Panels.Count > Capacity)
            {
                Panels.RemoveAt(0);
            }
        }

        note("PanelGuid", guid);
        note("PanelIndex", Panels.Count - 1);
        note("Args", arguments.Select(Describe).ToList());
    }

    public static string List()
    {
        return Json.Serialize(Panels.Select((panel, index) => new Dictionary<string, object?>
        {
            ["index"] = index,
            ["utc"] = panel.Utc.ToString("HH:mm:ss.fff"),
            ["guid"] = panel.Guid,
            ["anchor"] = panel.Anchor,
            ["rebind"] = panel.Rebind,
            ["args"] = panel.Arguments.Select(Describe).ToList()
        }).ToList());
    }

    // index: which captured panel (default the last); argN.key / argN.path replace argument N's key or path.
    public static Task<string> OpenAsync(int? index, IReadOnlyDictionary<string, string> overrides, bool send)
    {
        return MainThreadDispatcher.RunAsync(() =>
        {
            if (Panels.Count == 0)
            {
                throw new ArgumentException("No panel captured: start /events, open the panel in the game by hand, then retry.");
            }

            var chosen = Panels[index ?? Panels.Count - 1];
            var arguments = new Il2CppSystem.Collections.Generic.List<IOpenPanelEvent.Argument>();
            var sent = new List<Dictionary<string, object?>>();

            for (var i = 0; i < chosen.Arguments.Count; i++)
            {
                var recorded = chosen.Arguments[i];
                var key = overrides.TryGetValue($"arg{i}.key", out var k) ? ulong.Parse(k) : recorded.Key;
                var path = overrides.TryGetValue($"arg{i}.path", out var p) ? p : recorded.Path;
                var argument = new IOpenPanelEvent.Argument
                {
                    name = recorded.Name,
                    key = new Bindings.Key(key),
                    path = path,
                    method = (BindMethod)recorded.Method
                };

                arguments.Add(argument);
                sent.Add(Describe(recorded with { Key = key, Path = path }));
            }

            if (send)
            {
                Subsystems.Get<EventSubsystem>().SendEvent(new OpenPanelEvent
                {
                    _PanelID_k__BackingField = chosen.Panel,
                    _Arguments_k__BackingField = arguments,
                    _Anchor_k__BackingField = chosen.Anchor ?? "",
                    _RebindMethod_k__BackingField = (RebindMethod)chosen.Rebind,
                    _CheckForExistingOpenedPanel_k__BackingField = false
                });
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["event"] = "SI.Bindable.Events.OpenPanelEvent",
                ["guid"] = chosen.Guid,
                ["args"] = sent,
                ["sent"] = send
            });
        });
    }

    private static Dictionary<string, object?> Describe(Argument argument)
    {
        return new Dictionary<string, object?>
        {
            ["name"] = argument.Name,
            ["key"] = argument.Key,
            ["path"] = argument.Path,
            ["method"] = argument.Method
        };
    }
}
