# Smoke for ticket #74 (staff actions). Runs on the Windows PC, with the API (5120) built from agent/t74 and the test
# save "MCPFM investigation" loaded, outside a live match. Without -Apply it only reads and runs dry runs.
#   powershell -File scripts\smoke-74.ps1 [-Apply] [-Repeat 2]
param([switch]$Apply, [int]$Repeat = 2)
$ErrorActionPreference = 'Stop'
$api = 'http://localhost:5120/api/v1'

function Post($path, $body) {
    try {
        $r = Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$api$path" -ContentType 'application/json' -Body ($body | ConvertTo-Json -Compress) -TimeoutSec 120
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

function AssertTestSave {
    $s = Invoke-RestMethod -UseBasicParsing 'http://localhost:5121/session' -TimeoutSec 20
    if (-not $s.isInvestigationSave) { throw "Not the test save: stop." }
    $live = Invoke-RestMethod -UseBasicParsing "$api/matches/live" -TimeoutSec 20
    if ($live.inProgress) { throw "A match is live: stop." }
}

$failed = 0
AssertTestSave
$staff = Invoke-RestMethod -UseBasicParsing "$api/staff" -TimeoutSec 60
$member = $staff.members | Where-Object { $_.job -notmatch 'Técnico|Manager' } | Select-Object -First 1
Write-Output "Staff member for the menu: $($member.name) ($($member.id), $($member.job)); vacancies $($staff.vacancies)"

for ($i = 1; $i -le $Repeat; $i++) {
    # Use case 4: a staff member's menu, dry run only.
    foreach ($action in 'Dismiss', 'MutualTermination', 'CoachingCourse', 'LeadershipCourse') {
        $r = Post '/staff/actions' @{ staffId = $member.id; action = $action }
        Check "member $action dry run #$i" ($r.Status -eq 200 -or ($r.Status -eq 400 -and $r.Body -match "doesn't offer")) "$($r.Status) $($r.Body.Substring(0, [Math]::Min(300, $r.Body.Length)))"
    }

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
        $entry = if ($plan.choice) { $plan.choice -split ' > ' } else { @($plan.menu[0].label) }
        $r = Post '/staff/actions?apply=true' @{ action = 'AdvertiseCoachingJob'; entry = $entry }
        $now = Invoke-RestMethod -UseBasicParsing "$api/staff" -TimeoutSec 60
        Check "advert apply #$i" ($r.Status -eq 200) "$($r.Status) vacancies $after -> $($now.vacancies) :: $($r.Body)"
    }
}

Write-Output "Failures: $failed"
exit $failed
