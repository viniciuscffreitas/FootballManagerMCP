# Smoke for ticket #75 (board: requests, budget split, talks). Runs on the Windows PC, with the API (5120) built from
# agent/t75 and the test save "MCPFM investigation" loaded, outside a live match. Without -Apply it only reads and runs
# dry runs; -Apply also moves the budget split there and back (x$Repeat) and sends one cheap request (an affiliate).
#   powershell -File scripts\smoke-75.ps1 [-Apply] [-Repeat 2] [-Steps 1] [-Request AffiliateLink]
param([switch]$Apply, [int]$Repeat = 2, [int]$Steps = 1, [string]$Request = 'AffiliateLink')
$ErrorActionPreference = 'Stop'
$api = 'http://localhost:5120/api/v1'

function Post($path, $body) {
    try {
        $json = [Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Compress))
        $r = Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$api$path" -ContentType 'application/json; charset=utf-8' -Body $json -TimeoutSec 120
        return @{ Status = [int]$r.StatusCode; Body = $r.Content; Json = ($r.Content | ConvertFrom-Json) }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        return @{ Status = [int]$resp.StatusCode; Body = $text; Json = $null }
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

function Board { Invoke-RestMethod -UseBasicParsing "$api/board" -TimeoutSec 60 }

$failed = 0
AssertTestSave
$before = Board
Write-Output "get_board before: boardRequests $($before.boardRequests), active $($before.activeBoardRequests)"

for ($i = 1; $i -le $Repeat; $i++) {
    # UC1: the requests on offer, dry run.
    $r = Post '/board/requests?apply=false' @{ action = 'IncreaseBudgets' }
    Check "UC1 dry run #$i" ($r.Status -eq 200 -and -not $r.Json.applied -and $r.Json.menu.Count -ge 2) "offered $($r.Json.offered -join ','); menu $(($r.Json.menu | % label) -join ' | ')"

    # EC1: an entry the menu doesn't have.
    AssertTestSave
    $r = Post '/board/requests?apply=true' @{ action = 'IncreaseBudgets'; entry = 'Comprar um estadio' }
    Check "EC1 unknown entry #$i" ($r.Status -eq 400) $r.Body

    # EC2: answering with no talk open.
    AssertTestSave
    $r = Post '/board/conversation?apply=true' @{ line = '0' }
    Check "EC2 no talk open #$i" ($r.Status -eq 400) $r.Body

    # Budget dry run.
    $r = Post '/board/budget?apply=false' @{ steps = $Steps }
    Check "Budget dry run #$i" ($r.Status -eq 200 -and -not $r.Json.applied -and $r.Json.budgets) $r.Json.budgets
}

# EC3: several entries, none named.
AssertTestSave
$r = Post '/board/requests?apply=true' @{ action = 'IncreaseBudgets' }
Check "EC3 several entries" ($r.Status -eq 400) $r.Body

$after = Board
Check "get_board unchanged by dry runs" (($after | ConvertTo-Json -Depth 6) -eq ($before | ConvertTo-Json -Depth 6)) "boardRequests $($after.boardRequests)"

if ($Apply) {
    for ($i = 1; $i -le $Repeat; $i++) {
        # UC2: transfers -> wages, then back.
        AssertTestSave
        $open = Post '/board/budget?apply=true' @{}
        Check "UC2 open panel #$i" ($open.Status -eq 200 -and $open.Json.before) ($open.Json.before | ConvertTo-Json -Compress)
        $start = $open.Json.before
        AssertTestSave
        $to = Post '/board/budget?apply=true' @{ steps = $Steps }
        Check "UC2 move +$Steps #$i" ($to.Status -eq 200 -and $to.Json.after.transferBudget -ne $start.transferBudget) ($to.Json.after | ConvertTo-Json -Compress)
        AssertTestSave
        $back = Post '/board/budget?apply=true' @{ steps = -$Steps }
        Check "UC2 move back #$i" ($back.Status -eq 200 -and $back.Json.after.transferBudget -eq $start.transferBudget -and $back.Json.after.wageBudget -eq $start.wageBudget) ($back.Json.after | ConvertTo-Json -Compress)
    }

    # UC3: one cheap request (the board hears one at a time): an affiliate, pitched in the board meeting line by line,
    # then out of the meeting.
    AssertTestSave
    $r = Post '/board/requests?apply=true' @{ action = $Request }
    Check "UC3 open the board meeting" ($r.Status -eq 200 -and $r.Json.applied -and $r.Json.card.lines.Count -gt 0) "card $(($r.Json.card.lines) -join ' | ')"
    for ($round = 1; $round -le 8 -and $r.Status -eq 200; $round++) {
        AssertTestSave
        $talk = Post '/board/conversation?apply=false' @{}
        if ($talk.Status -ne 200) { break }
        # The first line pitches the request; once the talk is over only "Sair da reunião" is left, and it leaves.
        $r = Post '/board/conversation?apply=true' @{ line = '0' }
        Write-Output "   said '$($r.Json.line)'; next: $(($r.Json.after.lines) -join ' | ')"
        if (-not $r.Json.after) { break }
    }
    Start-Sleep 2
    $final = Board
    Check "UC3 get_board shows the request" ($final.boardRequests -gt $before.boardRequests -or $final.activeBoardRequests -gt $before.activeBoardRequests) "boardRequests $($final.boardRequests), active $($final.activeBoardRequests)"
}

Write-Output "failed: $failed"
exit $failed
