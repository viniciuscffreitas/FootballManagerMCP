# Smoke for ticket #67 (instructions against the next opponent). Runs on the Windows PC, with the API (5120) built
# from agent/t67 and the test save "MCPFM investigation" loaded, outside a live match. Without -Apply it only runs dry
# runs; with -Apply it also gives one opponent player tight marking, checks it by the read back, and clears it.
#   powershell -File scripts\smoke-67.ps1 [-Apply] [-Repeat 2]
param([switch]$Apply, [int]$Repeat = 2)
$ErrorActionPreference = 'Stop'
$api = 'http://localhost:5120/api/v1'

function Post($path, $body) {
    try {
        $r = Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$api$path" -ContentType 'application/json' -Body ($body | ConvertTo-Json -Compress) -TimeoutSec 120
        return @{ Status = [int]$r.StatusCode; Body = $r.Content; Json = ($r.Content | ConvertFrom-Json) }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        return @{ Status = [int]$resp.StatusCode; Body = $text; Json = $null }
    }
}

function Check($name, $ok, $detail) {
    $mark = if ($ok) { 'PASS' } else { 'FAIL' }
    $short = if ($detail.Length -gt 400) { $detail.Substring(0, 400) } else { $detail }
    Write-Output "$mark $name :: $short"
    if (-not $ok) { $script:failed++ }
}

function AssertTestSave {
    $s = Invoke-RestMethod -UseBasicParsing 'http://localhost:5121/session' -TimeoutSec 20
    if (-not $s.isInvestigationSave) { throw "Not the test save: stop." }
    $live = Invoke-RestMethod -UseBasicParsing "$api/matches/live" -TimeoutSec 20
    if ($live.inProgress) { throw "A match is live: stop." }
}

# The instructions as a dry run reads them, as text to compare before and after.
function Snapshot {
    $r = Post '/opposition/instructions' @{ action = 'ApplyStaffAdvice' }
    if ($r.Status -ne 200) { throw "Dry run failed: $($r.Status) $($r.Body)" }
    return ($r.Json.current.players | ConvertTo-Json -Compress -Depth 5)
}

function Row($change, $id) {
    return @($change.players) | Where-Object { $_.playerId -eq $id } | Select-Object -First 1
}

$failed = 0
AssertTestSave

$first = Post '/opposition/instructions' @{ action = 'ApplyStaffAdvice' }
if ($first.Status -ne 200) { throw "No instructions to test: $($first.Status) $($first.Body)" }
$current = $first.Json.current
# The staff's first suggested player when the lineup has him, else the expected lineup's last player (a forward).
$suggested = @($current.staffSuggestions | Where-Object { $_.playerId }) | Select-Object -First 1
$target = if ($suggested -and (Row $current $suggested.playerId)) { Row $current $suggested.playerId } else { @($current.players)[-1] }
Write-Output "Opponent $($current.teamName) ($($current.teamId)); target $($target.name) ($($target.playerId), $($target.position)); marking now $($target.marking)"
Write-Output "Staff suggestions: $(@($current.staffSuggestions).Count); lineup rows $(@($current.players).Count); position rows $(@($current.positions).Count)"

for ($i = 1; $i -le $Repeat; $i++) {
    # Use case 1: dry run of a Set lists the instructions, the staff's suggestions and the plan, and changes nothing.
    $before = Snapshot
    $r = Post '/opposition/instructions' @{ action = 'Set'; player = "$($target.playerId)"; marking = 'always' }
    $after = Snapshot
    Check "set dry run #$i" ($r.Status -eq 200 -and -not $r.Json.applied -and @($r.Json.current.players).Count -gt 0 -and $before -eq $after) `
        "$($r.Status) plan=$($r.Json.plan | ConvertTo-Json -Compress) suggestions=$(@($r.Json.current.staffSuggestions).Count) unchanged=$($before -eq $after)"

    # Use case 3: dry run of the staff's advice lists it and changes nothing.
    $r = Post '/opposition/instructions' @{ action = 'ApplyStaffAdvice' }
    Check "advice dry run #$i" ($r.Status -eq 200 -and -not $r.Json.applied -and (Snapshot) -eq $after) `
        "$($r.Status) plan=$($r.Json.plan | ConvertTo-Json -Compress) notes=$($r.Json.notes -join ' | ')"

    # Edge case 2: no action named is refused, never read as the first one.
    $r = Post '/opposition/instructions?apply=true' @{ player = "$($target.playerId)"; marking = 'always' }
    Check "no action #$i" ($r.Status -eq 400 -and $r.Body -match 'action' -and (Snapshot) -eq $after) "$($r.Status) $($r.Body)"

    if (-not $Apply) { continue }

    AssertTestSave
    # Edge case 1: a player outside the expected lineup is refused and nothing changes.
    $r = Post '/opposition/instructions?apply=true' @{ action = 'Set'; player = 'Neymar'; marking = 'always' }
    Check "outside the lineup #$i" ($r.Status -eq 400 -and (Snapshot) -eq $after) "$($r.Status) $($r.Body)"

    # Use case 2: tight marking on the target, proven by the read back, then cleared, proven again.
    AssertTestSave
    $r = Post '/opposition/instructions?apply=true' @{ action = 'Set'; player = "$($target.playerId)"; marking = 'always' }
    $read = Row $r.Json.after $target.playerId
    Check "set apply #$i" ($r.Status -eq 200 -and $r.Json.applied -and $read.marking -eq 'always' -and @($r.Json.plan | Where-Object { $_.confirmed -ne $true }).Count -eq 0) `
        "$($r.Status) after.marking=$($read.marking) plan=$($r.Json.plan | ConvertTo-Json -Compress) notes=$($r.Json.notes -join ' | ')"

    AssertTestSave
    $r = Post '/opposition/instructions?apply=true' @{ action = 'Clear'; player = "$($target.playerId)" }
    $read = Row $r.Json.after $target.playerId
    Check "clear apply #$i" ($r.Status -eq 200 -and $r.Json.applied -and $read.marking -eq 'none' -and $read.tackling -eq 'none' -and $read.pressing -eq 'none' -and $read.showOntoFoot -eq 'none') `
        "$($r.Status) after=$($read | ConvertTo-Json -Compress)"
}

# Put back what the target had before the smoke, when he had anything.
if ($Apply) {
    $back = @{ action = 'Set'; player = "$($target.playerId)" }
    foreach ($k in 'marking', 'pressing', 'tackling', 'showOntoFoot') { if ($target.$k -ne 'none') { $back[$k] = $target.$k } }
    if ($back.Count -gt 2) {
        $r = Post '/opposition/instructions?apply=true' $back
        Write-Output "Restored $($target.name): $($r.Status) $($r.Json.plan | ConvertTo-Json -Compress)"
    }
}

Write-Output "Failures: $failed"
exit $failed
