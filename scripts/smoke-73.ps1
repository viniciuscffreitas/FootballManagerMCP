# Smoke for ticket #73 (registration and squad numbers). Runs on the Windows PC, with the API (5120) built from
# agent/t73 and the test save "MCPFM investigation" loaded, outside a live match. Without -Apply it only reads and runs
# dry runs. Every pass checks the effect by reading it back; an HTTP 200 alone never passes.
#   powershell -File scripts\smoke-73.ps1 [-Apply] [-Repeat 2] [-Competition "Copa do Brasil"]
param([switch]$Apply, [int]$Repeat = 2, [string]$Competition = 'Copa do Brasil')
$ErrorActionPreference = 'Stop'
$api = 'http://localhost:5120/api/v1'

function Post($path, $body) {
    try {
        $r = Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$api$path" -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Compress))) -TimeoutSec 120
        return @{ Status = [int]$r.StatusCode; Body = $r.Content }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        return @{ Status = [int]$resp.StatusCode; Body = $text }
    }
}

function Check($name, $ok, $detail) {
    $mark = if ($ok) { 'PASS' } else { 'FAIL' }
    $text = "$detail"
    Write-Output "$mark $name :: $($text.Substring(0, [Math]::Min(400, $text.Length)))"
    if (-not $ok) { $script:failed++ }
}

function AssertTestSave {
    $s = Invoke-RestMethod -UseBasicParsing 'http://localhost:5121/session' -TimeoutSec 20
    if (-not $s.isInvestigationSave) { throw "Not the test save: stop." }
    $live = Invoke-RestMethod -UseBasicParsing "$api/matches/live" -TimeoutSec 20
    if ($live.inProgress) { throw "A match is live: stop." }
}

function Registration { Invoke-RestMethod -UseBasicParsing "$api/squad/registration" -TimeoutSec 120 }
function Cup($reg) { $reg.competitions | Where-Object { $_.competition -eq $Competition -or $_.name -eq $Competition } | Select-Object -First 1 }

$failed = 0
AssertTestSave

for ($i = 1; $i -le $Repeat; $i++) {
    # Use case 1: every competition with its rules and lists.
    $reg = Registration
    $cup = Cup $reg
    Check "read #$i" ($reg.competitions.Count -gt 0 -and $cup -and $cup.registered.Count -gt 0) "competitions $($reg.competitions.Count); $($cup.name): open $($cup.windowOpen), registered $($cup.registered.Count), unregistered $($cup.unregistered.Count), rules $($cup.rules.Count)"
    $rule = $cup.rules | Where-Object { $_.type -eq 9 } | Select-Object -First 1
    Check "rule value matches the list #$i" ($rule -and $rule.value -eq $cup.registered.Count) "max-size rule value $($rule.value), registered $($cup.registered.Count)"

    # Edge case 2: bad requests are refused without writing.
    $r = Post '/squad/registration?apply=true' @{ register = @(1) }
    Check "no competition #$i" ($r.Status -eq 400) $r.Body
    $r = Post '/squad/registration?apply=true' @{ competition = $Competition; register = @(1) }
    Check "player not in the lists #$i" ($r.Status -eq 400) $r.Body
    $r = Post '/squad/registration?apply=true' @{ competition = $Competition; register = @($null) }
    Check "empty item #$i" ($r.Status -eq 400) $r.Body

    # Use case 2: a dry run that registers someone changes nothing.
    $out = $cup.registered | Where-Object { -not $_.isGoalkeeper } | Select-Object -Last 1
    $dry = Post '/squad/registration' @{ competition = $Competition; unregister = @($out.id) }
    $after = Cup (Registration)
    Check "unregister dry run #$i" ($dry.Status -eq 200 -and $after.registered.Count -eq $cup.registered.Count) "$($dry.Status) $($dry.Body)"

    # Edge case 1: a change that breaks the goalkeeper minimum warns in the dry run and is refused on apply.
    $keepers = @($cup.registered | Where-Object { $_.isGoalkeeper })
    $minKeepers = $cup.rules | Where-Object { $_.type -eq 87 } | Select-Object -First 1
    if ($minKeepers) {
        $need = $keepers.Count - [int]($minKeepers.display -replace '^\d+/(\d+).*$', '$1') + 1
        $ids = @($keepers | Select-Object -First $need | ForEach-Object { $_.id })
        $r = Post '/squad/registration' @{ competition = $Competition; unregister = $ids }
        Check "rule break warns #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).warnings.Count -gt 0) $r.Body
    }

    if ($Apply -and $cup.windowOpen) {
        AssertTestSave
        $r = Post '/squad/registration?apply=true' @{ competition = $Competition; unregister = @($out.id) }
        $now = Cup (Registration)
        $gone = -not ($now.registered | Where-Object { $_.id -eq $out.id })
        Check "unregister apply #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).confirmed -and $gone) "$($r.Status) $($r.Body)"

        AssertTestSave
        $r = Post '/squad/registration?apply=true' @{ competition = $Competition; register = @($out.id) }
        $now = Cup (Registration)
        $back = [bool]($now.registered | Where-Object { $_.id -eq $out.id })
        Check "register apply #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).confirmed -and $back) "$($r.Status) $($r.Body)"
    }

    # Use case 3 and edge cases 2-3: squad numbers.
    $squad = (Invoke-RestMethod -UseBasicParsing "$api/squad" -TimeoutSec 120).players
    $reserve = $squad | Select-Object -Last 1
    $r = Post '/squad/numbers' @{}
    Check "number without a request #$i" ($r.Status -eq 400) $r.Body
    $r = Post '/squad/numbers' @{ askAssistant = $true }
    Check "assistant dry run #$i" ($r.Status -eq 200 -and -not ($r.Body | ConvertFrom-Json).applied) $r.Body
    $taken = ($cup.registered | Where-Object { $_.squadNumber -and $_.id -ne $reserve.id } | Select-Object -First 1).squadNumber
    $r = Post '/squad/numbers' @{ playerId = $reserve.id; number = $taken }
    Check "taken number #$i" ($r.Status -eq 400) $r.Body

    if ($Apply) {
        $used = @($cup.registered + $cup.unregistered | ForEach-Object { $_.squadNumber }) | Where-Object { $_ }
        $free = (90..99 | Where-Object { $used -notcontains $_ } | Select-Object -First 1)
        $old = ($cup.registered + $cup.unregistered | Where-Object { $_.id -eq $reserve.id } | Select-Object -First 1).squadNumber
        AssertTestSave
        $r = Post '/squad/numbers?apply=true' @{ playerId = $reserve.id; number = $free }
        $body = $r.Body | ConvertFrom-Json
        $read = (Cup (Registration)).registered + (Cup (Registration)).unregistered | Where-Object { $_.id -eq $reserve.id } | Select-Object -First 1
        Check "number apply #$i" ($r.Status -eq 200 -and $body.confirmed -and $read.squadNumber -eq $free) "$($reserve.name): $old -> $free, read $($read.squadNumber) :: $($r.Body)"
        if ($old) {
            AssertTestSave
            $r = Post '/squad/numbers?apply=true' @{ playerId = $reserve.id; number = $old }
            Check "number back #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).confirmed) $r.Body
        }
    }
}

Write-Output "Failures: $failed"
exit $failed
