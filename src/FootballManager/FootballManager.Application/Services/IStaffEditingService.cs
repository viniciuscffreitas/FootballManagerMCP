using FluentResults;
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
