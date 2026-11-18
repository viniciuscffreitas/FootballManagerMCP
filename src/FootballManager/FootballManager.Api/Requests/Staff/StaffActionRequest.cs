using FootballManager.Domain.Staff;

namespace FootballManager.Api.Requests.Staff;

public class StaffActionRequest
{
    /// <summary>The staff member's FM id from get_staff; left out for a job advert, which is the club's.</summary>
    public int? StaffId { get; set; }

    /// <summary>Required: left out, it is refused rather than read as the enum's first value (Dismiss).</summary>
    public StaffMenuAction? Action { get; set; }

    /// <summary>The menu entry to pick, by its label at each level, as the dry run lists them.</summary>
    public List<string>? Entry { get; set; }
}
