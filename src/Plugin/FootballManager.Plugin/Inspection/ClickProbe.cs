using FootballManager.Plugin.Data;
using FootballManager.Plugin.Writes;
using SI.Bindable;
using UnityEngine.UIElements;

namespace FootballManager.Plugin.Inspection;

// Experiment: presses a button of the game's UI from inside the game, without a mouse, the way the game's own
// keyboard and controller navigation does. The element is a /ui path; the button is it or its nearest SIButton
// ancestor. "simulate" calls SIButton.SimulateButtonClick, "submit" calls SIButton.Submit, "navsubmit" sends a
// NavigationSubmitEvent to the element.
internal static class ClickProbe
{
    public static async Task<string> ClickAsync(string? path, string? how, bool confirmed, bool dryRun, TimeSpan timeout)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Pass path=, a /ui path such as 0/3/0/1.");
        }

        how ??= "simulate";

        if (how is not ("simulate" or "submit" or "navsubmit"))
        {
            throw new ArgumentException("how= is simulate, submit or navsubmit.");
        }

        if (!dryRun)
        {
            await WriteGuard.EnsureAllowedAsync(confirmed, timeout);
        }

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var element = UiDocuments.Resolve(path);
            var button = FindButton(element);
            var target = button is null ? "none" : $"{button.GetIl2CppType().Name}#{button.name} '{button.text}'";

            if (!dryRun)
            {
                switch (how)
                {
                    case "simulate":
                        (button ?? throw new ArgumentException($"No SIButton at or above {path}.")).SimulateButtonClick(false);
                        break;
                    case "submit":
                        (button ?? throw new ArgumentException($"No SIButton at or above {path}.")).Submit();
                        break;
                    default:
                        using (var submit = NavigationSubmitEvent.GetPooled())
                        {
                            submit.target = element;
                            element.SendEvent(submit);
                        }

                        break;
                }
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["element"] = $"{element.GetIl2CppType().Name}#{element.name}",
                ["button"] = target,
                ["canSubmit"] = button?.CanSubmit,
                ["how"] = how,
                ["pressed"] = !dryRun
            });
        });
    }

    private static SIButton? FindButton(VisualElement? element)
    {
        while (element is not null)
        {
            if (element.TryCast<SIButton>() is { } button)
            {
                return button;
            }

            element = element.hierarchy.parent;
        }

        return null;
    }
}
