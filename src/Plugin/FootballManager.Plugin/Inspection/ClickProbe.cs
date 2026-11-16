using FootballManager.Plugin.Data;
using FootballManager.Plugin.Writes;
using SI.Bindable;
using UnityEngine.UIElements;

namespace FootballManager.Plugin.Inspection;

// Experiment: presses a button of the game's UI from inside the game, without a mouse, the way the game's own
// keyboard and controller navigation does. The element is a /ui path; the button is it or its nearest SIButton
// ancestor, or else its nearest SIRadioButton (pressed through its own OnClickedOrSubmitted), SIDropdown (opened with
// ShowDropdown, or set with index=), SIFloatingPopupWidget (opened with ShowFloatingPart) or, failing those, any
// NavigatableVisualElement such as a NavigableLink (submitted as keyboard navigation does). "simulate" calls SIButton.SimulateButtonClick, "submit" calls SIButton.Submit.
internal static class ClickProbe
{
    public static async Task<string> ClickAsync(string? path, string? how, bool confirmed, bool dryRun, TimeSpan timeout, int? index = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Pass path=, a /ui path such as 0/3/0/1.");
        }

        how ??= "simulate";

        if (how is not ("simulate" or "submit"))
        {
            throw new ArgumentException("how= is simulate or submit.");
        }

        if (!dryRun)
        {
            await WriteGuard.EnsureAllowedAsync(confirmed, timeout);
        }

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var element = UiDocuments.Resolve(path);
            var button = FindButton(element);
            var radio = button is null ? FindRadio(element) : null;
            var dropdown = button is null && radio is null ? Find<SIDropdown>(element) : null;
            var popup = button is null && radio is null && dropdown is null ? Find<SIFloatingPopupWidget>(element) : null;
            var navigable = button is null && radio is null && dropdown is null && popup is null
                ? Find<SI.UI.NavigatableVisualElement>(element) : null;
            var target = button is not null
                ? $"{button.GetIl2CppType().Name}#{button.name} '{button.text}'"
                : radio is not null ? $"SIRadioButton#{radio.name} value={radio.value}"
                : dropdown is not null ? $"SIDropdown#{dropdown.name} index={dropdown.SelectedIndex}"
                : popup is not null ? $"SIFloatingPopupWidget#{popup.name}"
                : navigable is not null ? $"{navigable.GetIl2CppType().Name}#{navigable.name} canSubmit={navigable.CanSubmit}" : "none";

            if (!dryRun && radio is not null)
            {
                radio.OnClickedOrSubmitted();
            }
            else if (!dryRun && dropdown is not null)
            {
                if (index is { } chosen)
                {
                    // Picks an item by its place in the list, as choosing it in the open list does.
                    dropdown.SelectedIndex = chosen;
                    dropdown.SetSelectedValueByIndex();
                }
                else
                {
                    dropdown.ShowDropdown();
                }
            }
            else if (!dryRun && popup is not null)
            {
                popup.ShowFloatingPart();
            }
            else if (!dryRun && navigable is not null)
            {
                // A link follows itself through its own handler; any other control (a tile) is submitted as keyboard and
                // controller navigation do.
                if (navigable.TryCast<NavigableLink>() is { } link)
                {
                    link.OnClickedOrSubmitted(link);
                }
                else
                {
                    navigable.Submit();
                }
            }
            else if (!dryRun)
            {
                switch (how)
                {
                    case "simulate":
                        (button ?? throw new ArgumentException($"No SIButton at or above {path}.")).SimulateButtonClick(false);
                        break;
                    default:
                        (button ?? throw new ArgumentException($"No SIButton at or above {path}.")).Submit();
                        break;
                }
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["element"] = $"{element.GetIl2CppType().Name}#{element.name}",
                ["button"] = target,
                ["canSubmit"] = button?.CanSubmit,
                ["radioValue"] = radio?.value,
                ["how"] = how,
                ["pressed"] = !dryRun
            });
        });
    }

    private static T? Find<T>(VisualElement? element) where T : Il2CppSystem.Object
    {
        while (element is not null)
        {
            if (element.TryCast<T>() is { } found)
            {
                return found;
            }

            element = element.hierarchy.parent;
        }

        return null;
    }

    private static SIRadioButton? FindRadio(VisualElement? element)
    {
        while (element is not null)
        {
            if (element.TryCast<SIRadioButton>() is { } radio)
            {
                return radio;
            }

            element = element.hierarchy.parent;
        }

        return null;
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
