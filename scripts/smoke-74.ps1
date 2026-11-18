# Smoke for ticket #74 (staff actions). Runs on the Windows PC, with the API (5120) built from agent/t74 and the test
# save "MCPFM investigation" loaded, outside a live match. Without -Apply it only reads and runs dry runs.
#   powershell -File scripts\smoke-74.ps1 [-Apply] [-Repeat 2]
param([switch]$Apply, [int]$Repeat = 2)
$ErrorActionPreference = 'Stop'
$api = 'http://localhost:5120/api/v1'

function Post($path, $body) {
    try {
        # Windows PowerShell 5.1 sends a string body as Latin-1: send UTF-8 bytes, or the menu's accented labels break.
        $bytes = [Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Compress))
        $r = Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$api$path" -ContentType 'application/json; charset=utf-8' -Body $bytes -TimeoutSec 120
        return @{ Status = [int]$r.StatusCode; Body = $r.Content }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        return @{ Status = [int]$resp.StatusCode; Body = $text }
    }
}

function Check($name, $ok, $detail) {
    $mark = if ($ok) { 'PASS' } else { 'FAIL' }
    Write-Output "$mark $name :: $detail"
    if (-not $ok) { $script:failed++ }
}

# Every entry of the menu that can be picked, by its full path ("Publicar Anúncio > Treino > Treinador"): one without
# sub-entries the game doesn't grey out. The service takes only such a path, never a sub-menu's label.
function Leaves($entries, $above) {
    foreach ($e in @($entries)) {
        # A greyed-out entry is skipped with everything under it, as the API does.
        if ($e.disabled) { continue }
        $path = if ($above) { "$above > $($e.label)" } else { $e.label }
        if (@($e.children).Count -gt 0) { Leaves $e.children $path }
        else { $path }
    }
}

function AssertTestSave {
    $s = Invoke-RestMethod -UseBasicParsing 'http://localhost:5121/session' -TimeoutSec 20
    if (-not $s.isInvestigationSave) { throw "Not the test save: stop." }
    $live = Invoke-RestMethod -UseBasicParsing "$api/matches/live" -TimeoutSec 20
    if ($live.inProgress) { throw "A match is live: stop." }
}

$failed = 0
AssertTestSave
$staff = Invoke-RestMethod -UseBasicParsing "$api/staff" -TimeoutSec 60
# Skip the manager: his menu has none of the member actions. Windows PowerShell 5.1 reads this file as ANSI, so the
# pattern avoids the accented letter of "Técnico".
$member = $staff.members | Where-Object { $_.job -notmatch '^T\S{1,2}cnico$|^Manager$' } | Select-Object -First 1
Write-Output "Staff member for the menu: $($member.name) ($($member.id), $($member.job)); vacancies $($staff.vacancies)"

for ($i = 1; $i -le $Repeat; $i++) {
    # Use case 4: a staff member's menu, dry run only.
    foreach ($action in 'Dismiss', 'MutualTermination', 'CoachingCourse', 'LeadershipCourse') {
        $r = Post '/staff/actions' @{ staffId = $member.id; action = $action }
        Check "member $action dry run #$i" ($r.Status -eq 200 -or ($r.Status -eq 400 -and $r.Body -match "doesn't offer")) "$($r.Status) $($r.Body.Substring(0, [Math]::Min(300, $r.Body.Length)))"
    }

    # Edge case 3: no action named is refused, never read as the first one (Dismiss).
    $r = Post '/staff/actions' @{ staffId = $member.id }
    Check "no action #$i" ($r.Status -eq 400 -and $r.Body -match 'action') $r.Body

    # Edge case 2: someone not on the staff, and an action that runs on the club given a member.
    $r = Post '/staff/actions' @{ staffId = 1; action = 'Dismiss' }
    Check "not on staff #$i" ($r.Status -eq 400 -and $r.Body -match 'not on the first team') $r.Body
    $r = Post '/staff/actions' @{ staffId = $member.id; action = 'AdvertiseCoachingJob' }
    Check "advert on a member #$i" ($r.Status -eq 400) $r.Body

    # Use case 3 / edge case 1: the coaching job advert, dry run first; then (with -Apply) placed and read back.
    $before = (Invoke-RestMethod -UseBasicParsing "$api/staff" -TimeoutSec 60).vacancies
    $dry = Post '/staff/actions' @{ action = 'AdvertiseCoachingJob' }
    $after = (Invoke-RestMethod -UseBasicParsing "$api/staff" -TimeoutSec 60).vacancies
    Check "advert dry run #$i" ($dry.Status -eq 200 -and $before -eq $after) "$($dry.Status) vacancies $before -> $after :: $($dry.Body)"

    if ($Apply -and $dry.Status -eq 200) {
        AssertTestSave
        $plan = $dry.Body | ConvertFrom-Json
        $leaf = if ($plan.choice) { $plan.choice } else { @(Leaves $plan.menu '')[0] }
        Write-Output "Advert entry picked: $leaf"
        $r = Post '/staff/actions?apply=true' @{ action = 'AdvertiseCoachingJob'; entry = @($leaf -split ' > ') }
        Start-Sleep -Seconds 2
        $applied = $r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).applied
        # The advert placed shows in the menu: the game greys its job out ("A vaga já foi anunciada."). StaffVacancies
        # counts the club's unfilled posts and doesn't move with an advert (test save, 18/11/2026).
        $again = Post '/staff/actions' @{ action = 'AdvertiseCoachingJob' }
        $greyed = $again.Status -eq 200 -and -not (@(Leaves ($again.Body | ConvertFrom-Json).menu '') -contains $leaf)
        Check "advert apply #$i" ($applied -and $greyed) "$($r.Status) greyed out after: $greyed :: $($r.Body)"
        # Edge: the job just advertised is greyed out, so asking for it again is refused before any write.
        AssertTestSave
        $twice = Post '/staff/actions?apply=true' @{ action = 'AdvertiseCoachingJob'; entry = @($leaf -split ' > ') }
        Check "advert again refused #$i" ($twice.Status -ge 400 -and $twice.Status -lt 500) "$($twice.Status) $($twice.Body)"
    }
}

Write-Output "Failures: $failed"
exit $failed
