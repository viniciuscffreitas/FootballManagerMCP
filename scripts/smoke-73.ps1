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
# The registered ids that differ between two reads, other than the player asked: a confirm sends every tick on the
# screen, so another player moving with him is a failure (seen 18/11/2026).
function Others($before, $after, $asked) {
    $a = @($before.registered | ForEach-Object { $_.id }); $b = @($after.registered | ForEach-Object { $_.id })
    @(Compare-Object $a $b | Where-Object { $_.InputObject -ne $asked } | ForEach-Object { $_.InputObject })
}

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

    # Edge case 1: unregistering down past the minimum squad size warns in the dry run and writes nothing.
    $minSize = $cup.rules | Where-Object { $_.type -eq 165 } | Select-Object -First 1
    if ($minSize) {
        $limit = [int]($minSize.display -replace '^\d+/(\d+).*$', '$1')
        $ids = @($cup.registered | Where-Object { -not $_.isGoalkeeper } | Select-Object -First ($cup.registered.Count - $limit + 1) | ForEach-Object { $_.id })
        $r = Post '/squad/registration' @{ competition = $Competition; unregister = $ids }
        $still = (Cup (Registration)).registered.Count
        Check "size rule break warns #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).warnings.Count -gt 0 -and $still -eq $cup.registered.Count) "unregister $($ids.Count): warnings $((($r.Body | ConvertFrom-Json).warnings) -join ' | '); registered still $still"
        $r = Post '/squad/registration?apply=true' @{ competition = $Competition; unregister = $ids }
        $still = (Cup (Registration)).registered.Count
        Check "size rule break refused on apply #$i" ($r.Status -eq 400 -and $still -eq $cup.registered.Count) "$($r.Status) registered still $still"
    }

    # Edge case 1b: a change that breaks the goalkeeper minimum warns in the dry run.
    $keepers = @($cup.registered | Where-Object { $_.isGoalkeeper })
    $minKeepers = $cup.rules | Where-Object { $_.type -eq 87 } | Select-Object -First 1
    if ($minKeepers) {
        $need = $keepers.Count - [int]($minKeepers.display -replace '^\d+/(\d+).*$', '$1') + 1
        $ids = @($keepers | Select-Object -First $need | ForEach-Object { $_.id })
        $r = Post '/squad/registration' @{ competition = $Competition; unregister = $ids }
        Check "rule break warns #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).warnings.Count -gt 0) $r.Body
    }

    # The use case needs an open window: with it closed, -Apply fails rather than passing without proof.
    if ($Apply -and -not $cup.windowOpen) {
        Check "window open for the apply #$i" $false "$($cup.name): $($cup.window)"
    }

    if ($Apply -and $cup.windowOpen) {
        AssertTestSave
        $was = Cup (Registration)
        $r = Post '/squad/registration?apply=true' @{ competition = $Competition; unregister = @($out.id) }
        $now = Cup (Registration)
        $gone = -not ($now.registered | Where-Object { $_.id -eq $out.id })
        $moved = Others $was $now $out.id
        Check "unregister apply #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).confirmed -and $gone -and $moved.Count -eq 0 -and $now.registered.Count -eq $was.registered.Count - 1) "$($r.Status) others moved [$($moved -join ',')] $($was.registered.Count)->$($now.registered.Count) $($r.Body)"

        AssertTestSave
        $was = $now
        $r = Post '/squad/registration?apply=true' @{ competition = $Competition; register = @($out.id) }
        $now = Cup (Registration)
        $back = [bool]($now.registered | Where-Object { $_.id -eq $out.id })
        $moved = Others $was $now $out.id
        Check "register apply #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).confirmed -and $back -and $moved.Count -eq 0 -and $now.registered.Count -eq $was.registered.Count + 1) "$($r.Status) others moved [$($moved -join ',')] $($was.registered.Count)->$($now.registered.Count) $($r.Body)"
    }

    # Use case 3 and edge cases 2-3: squad numbers.
    $squad = (Invoke-RestMethod -UseBasicParsing "$api/squad" -TimeoutSec 120).players
    # A reserve whose number the game lets change: in the test save (18/11/2026) only players without a number. Each
    # repetition takes the next one, since a player given a number is locked from then on.
    $candidates = @($squad | Where-Object { (Post '/squad/numbers' @{ playerId = $_.id; number = 97 }).Status -eq 200 })
    $reserve = $candidates[$i - 1]
    Write-Output "Number player #${i}: $($reserve.name) ($($reserve.id))"
    $r = Post '/squad/numbers' @{}
    Check "number without a request #$i" ($r.Status -eq 400) $r.Body
    $r = Post '/squad/numbers' @{ askAssistant = $true }
    Check "assistant dry run #$i" ($r.Status -eq 200 -and -not ($r.Body | ConvertFrom-Json).applied) $r.Body
    $taken = ($cup.registered | Where-Object { $_.squadNumber -and $_.id -ne $reserve.id } | Select-Object -First 1).squadNumber
    $r = Post '/squad/numbers' @{ playerId = $reserve.id; number = $taken }
    Check "taken number #$i" ($r.Status -eq 400 -and $r.Body -match 'taken') $r.Body

    if ($Apply) {
        # The first number from 90 the game offers him free (a dry run that passes).
        $free = 90..99 | Where-Object { (Post '/squad/numbers' @{ playerId = $reserve.id; number = $_ }).Status -eq 200 } | Select-Object -First 1
        $old = ($cup.registered + $cup.unregistered | Where-Object { $_.id -eq $reserve.id } | Select-Object -First 1).squadNumber
        AssertTestSave
        $r = Post '/squad/numbers?apply=true' @{ playerId = $reserve.id; number = $free }
        $body = $r.Body | ConvertFrom-Json
        # Read back through the API: the same number again is refused because the game shows he wears it now.
        $again = Post '/squad/numbers' @{ playerId = $reserve.id; number = $free }
        Check "number apply #$i" ($r.Status -eq 200 -and $body.confirmed -and $again.Status -eq 400 -and $again.Body -match 'already wears') "$($reserve.name): $old -> $free :: $($r.Body) :: read $($again.Body)"
        # The number given must not have been taken from anyone: no other first-team player wears it.
        $others = @($candidates | Where-Object { $_.id -ne $reserve.id } | Where-Object { (Post '/squad/numbers' @{ playerId = $_.id; number = $free }).Body -match 'taken' })
        Check "number free for the others #$i" ($others.Count -eq ($candidates.Count - 1)) "$($others.Count) of $($candidates.Count - 1) see $free taken"
        if ($old) {
            AssertTestSave
            $r = Post '/squad/numbers?apply=true' @{ playerId = $reserve.id; number = $old }
            Check "number back #$i" ($r.Status -eq 200 -and ($r.Body | ConvertFrom-Json).confirmed) $r.Body
        }
    }
}

Write-Output "Failures: $failed"
exit $failed
