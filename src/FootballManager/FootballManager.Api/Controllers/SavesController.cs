using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Saves;
using FootballManager.Api.Responses.Saves;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SavesController(ISaveGuard saveGuard) : ControllerBase
{
    /// <summary>The copies in the backup folder, newest first, with the club, copy time and game date their names carry.</summary>
    [HttpGet("backups")]
    public Ok<SaveBackupListResponse> GetBackups()
    {
        return TypedResults.Ok(saveGuard.ListBackups().Adapt<SaveBackupListResponse>());
    }

    /// <summary>
    /// Puts a copy back over the real save, after keeping the save as it is in the backup folder; refused while the
    /// real save is open in the game. A dry run unless apply=true.
    /// </summary>
    [HttpPost("restore")]
    public async Task<Results<Ok<SaveRestoreResponse>, BadRequest, NotFound, ProblemHttpResult>> RestoreAsync(
        [FromBody] RestoreSaveRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await saveGuard.RestoreAsync(request.Backup, apply, cancellationToken);
        return result.ToGetResult<SaveRestore, SaveRestoreResponse>(restore => restore.Adapt<SaveRestoreResponse>());
    }

    /// <summary>
    /// Clears the running game's version for writes on the real save. A dry run unless apply=true: the game's version
    /// against the released one.
    /// </summary>
    [HttpPost("release-version")]
    public async Task<Results<Ok<VersionReleaseResponse>, BadRequest, NotFound, ProblemHttpResult>> ReleaseVersionAsync(
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await saveGuard.ReleaseVersionAsync(apply, cancellationToken);
        return result.ToGetResult<VersionRelease, VersionReleaseResponse>(release => release.Adapt<VersionReleaseResponse>());
    }
}
