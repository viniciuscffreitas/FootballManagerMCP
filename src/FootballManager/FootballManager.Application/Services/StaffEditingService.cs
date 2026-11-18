using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Staff;

namespace FootballManager.Application.Services;

public interface IStaffEditingService
{
    /// <summary>
    /// Runs a staff menu action as the user would pick it in the game: on a first-team staff member (sack, mutual
    /// termination, a course, a new contract) or, with <paramref name="staffId"/> null, on the club (job adverts). A
    /// dry run unless <paramref name="apply"/>.
    /// </summary>
    /// <param name="entry">
    /// The menu entry to pick, by its label at each level (case doesn't matter). Needed to apply when the menu has more
    /// than one entry it can pick.
    /// </param>
    Task<Result<StaffAction>> RunActionAsync(
        int? staffId, StaffMenuAction? action, IReadOnlyList<string>? entry, bool apply, CancellationToken cancellationToken = default);
}

// Staff changes go through the menus the game builds for them, as the staff screen's do: the game offers only what fits
// the person now (no course for someone on one, no sacking for the user's own manager), each entry with the
// confirmation it would show. A dry run reads the menu and says what would be picked; nothing is written without apply.
public class StaffEditingService(IStaffEditorGameClient game, ISaveGuard saveGuard) : IStaffEditingService
{
    private const string Separator = " > ";

    // The game takes the change a frame or more after it's sent.
    private static readonly TimeSpan SettlePause = TimeSpan.FromMilliseconds(500);

    private static readonly HashSet<StaffMenuAction> OnTheClub =
    [
        StaffMenuAction.AdvertiseJob, StaffMenuAction.AdvertiseCoachingJob, StaffMenuAction.AdvertiseScoutingJob,
        StaffMenuAction.AdvertiseMedicalJob
    ];

    public async Task<Result<StaffAction>> RunActionAsync(
        int? staffId, StaffMenuAction? requested, IReadOnlyList<string>? entry, bool apply, CancellationToken cancellationToken = default)
    {
        // No default action: a request that leaves it out must not fall on the enum's first value (Dismiss).
        if (requested is not { } action)
        {
            return Fail("action", "Name the action to run, e.g. CoachingCourse or AdvertiseCoachingJob.");
        }

        if (OnTheClub.Contains(action) && staffId is not null)
        {
            return Fail("staffId", $"{action} runs on the club, not on a staff member: leave staffId out.");
        }

        if (!OnTheClub.Contains(action) && staffId is null)
        {
            return Fail("staffId", $"{action} runs on a staff member: give his FM id from get_staff.");
        }

        try
        {
            var plan = new StaffAction { Action = action, StaffId = staffId };

            if (staffId is { } id)
            {
                if (await game.FindMemberAsync(id, cancellationToken) is not { } member)
                {
                    return Fail("staffId", $"{id} is not on the first team's staff (get_staff lists who is).");
                }

                plan.StaffName = member.Name;
            }

            var offered = await game.GetOfferedActionsAsync(staffId, cancellationToken);

            if (!offered.Contains(action) || await game.GetMenuAsync(staffId, action, cancellationToken) is not { Count: > 0 } menu)
            {
                var others = offered.Count == 0 ? "none of the staff actions" : string.Join(", ", offered.Order());
                return Fail("action", $"The game's menu for {plan.StaffName ?? "the club"} doesn't offer {action} now; it offers {others}.");
            }

            plan.Menu = menu;
            var choices = Leaves(menu, []).ToList();
            var picked = entry is { Count: > 0 } ? Find(choices, entry) : choices.Count == 1 ? choices[0] : null;

            if (entry is { Count: > 0 } && picked is null)
            {
                return Fail("entry", $"The menu has no entry '{string.Join(Separator, entry)}' to pick; it has: {Labels(choices)}.");
            }

            if (picked is null && apply)
            {
                return Fail("entry", $"The menu has several entries: name the one to pick, among {Labels(choices)}.");
            }

            if (picked is { } chosen)
            {
                plan.Choice = chosen.Label;
                plan.Confirmation = chosen.Entry.Confirmation;
                plan.ConfirmationIsWarning = chosen.Entry.ConfirmationIsWarning;
            }

            if (!apply)
            {
                return Result.Ok(plan);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<StaffAction>(clearance.Errors);
            }

            plan.Notes.Add(SaveNote(clearance.Value));
            await game.RunMenuEntryAsync(staffId, action, picked.Value.Path, cancellationToken);
            plan.Applied = true;

            await Task.Delay(SettlePause, cancellationToken);
            plan.StillOffered = (await game.GetOfferedActionsAsync(staffId, cancellationToken)).Contains(action);
            return Result.Ok(plan);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<StaffAction>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Fail("game", exception.Message);
        }
    }

    // Every entry that does something when picked: one without sub-entries the game doesn't grey out.
    private static IEnumerable<(string Label, List<int> Path, MenuEntry Entry)> Leaves(List<MenuEntry> entries, List<string> above)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var labels = above.Append(entry.Label).ToList();

            if (entry.Children.Count > 0)
            {
                foreach (var (label, path, leaf) in Leaves(entry.Children, labels))
                {
                    yield return (label, path.Prepend(i).ToList(), leaf);
                }
            }
            else if (!entry.Disabled)
            {
                yield return (string.Join(Separator, labels), [i], entry);
            }
        }
    }

    private static (string Label, List<int> Path, MenuEntry Entry)? Find(
        List<(string Label, List<int> Path, MenuEntry Entry)> choices, IReadOnlyList<string> entry)
    {
        var wanted = string.Join(Separator, entry.Select(label => label.Trim()));

        foreach (var choice in choices)
        {
            if (string.Equals(choice.Label, wanted, StringComparison.CurrentCultureIgnoreCase))
            {
                return choice;
            }
        }

        return null;
    }

    private static string Labels(IEnumerable<(string Label, List<int> Path, MenuEntry Entry)> choices)
    {
        return string.Join("; ", choices.Select(choice => $"'{choice.Label}'"));
    }

    private static Result<StaffAction> Fail(string property, string message)
    {
        return Result.Fail<StaffAction>(new ValidationError(property, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
