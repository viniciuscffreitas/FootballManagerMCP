# Smoke #63: controlling the live match

Run on the test save only, with nobody else driving the game. Every call below must answer in 2 s or less; note
each time. Commands run on the Windows PC (`ssh -q windows`, PowerShell). `A` is the API, `B` the plugin's bridge.

```powershell
$A = "http://localhost:5120/api/v1"; $B = "http://localhost:5121"
function Post($path, $body) {
  $t = Measure-Command { $script:r = Invoke-WebRequest -UseBasicParsing -Method Post -ContentType "application/json" `
        -Body ($body | ConvertTo-Json -Compress) "$A/$path" -TimeoutSec 20 }
  "{0:N2} s" -f $t.TotalSeconds; $script:r.Content
}
function Live { $t = Measure-Command { $script:r = Invoke-WebRequest -UseBasicParsing "$A/matches/live" -TimeoutSec 20 }
  "{0:N2} s" -f $t.TotalSeconds; $script:r.Content }
```

## 0. Set up

1. `(Invoke-WebRequest -UseBasicParsing "$B/session").Content` shows `"isInvestigationSave": true`. Stop if not.
2. Put branch `agent/t63` on the PC and rebuild the API only (the plugin doesn't change, so FM keeps running):
   `Get-Process FootballManager.Api | Stop-Process -Force`, build, then `Start-ScheduledTask MCPFM-Api`.
3. Reach a match being played without Instant Result: `python C:\Code\pre.py` (stops once the match runs). Speed
   normal; don't use the Broadcast Suite manager camera (ALT), which makes every call slow.
4. `Live`: `inProgress` true, the minute, `onPitch`, `substitutionsRemaining` (e.g. 5), `tactic.mentality`.

## 1. Mentality (while the ball rolls)

1. Dry run: `Post "matches/live/mentality?apply=false" @{ mentality = "Attacking" }` → `applied` false, nothing changes
   on screen.
2. Unknown key: `Post "matches/live/mentality?apply=false" @{ mentality = "AllOut" }` → 400 listing the 7 keys.
3. Apply: `Post "matches/live/mentality?apply=true" @{ mentality = "Attacking" }` → `before` (e.g. `Positive`),
   `after` `Attacking`, `match.tactic.mentality` "Ofensivo". The match screen's mentality shows Ofensivo.
4. `Live` again: `tactic.mentality` "Ofensivo".

## 2. Substitution (while the ball rolls)

1. Find a bench player: `Post "matches/live/substitution?apply=false" @{ playerOffId = <id on the pitch>; playerOnId = 1 }`
   → 400 "isn't on the bench. On it now: <name (id)>, ...".
2. Dry run with both ids: `change` "<off> off, <on> on (place N)", note about the next stoppage, nothing on screen.
3. Apply: same with `apply=true` → `after` is the substitute's name (the tactic swapped them). The match screen shows
   the pending substitution.
4. Wait for the next stoppage (about a minute of play), then `Live`: `substitutions` lists the pair at that minute,
   `substitutionsRemaining` one lower, the substitute in `onPitch`.

## 3. Role (optional, not yet seen live)

1. `Post "matches/live/role?apply=false" @{ playerId = <id on the pitch>; inPossession = $true; role = "<key from get_tactic_options>" }`.
2. Apply → `before`/`after` role keys. Check in the match's tactics panel that the role changed. If it's refused with
   "The team's tactic doesn't have ...", note the place numbers: the match and the team's tactic differ there.

## 4. Half-time team talk

1. At half-time wait for the team talk: poll `(Invoke-WebRequest -UseBasicParsing "$B/get?root=continue&props=State").Content`
   every 5 s until `State` is `TeamTalk` (light /get; no /ui while the ball rolls).
2. During play the tool refuses: `Post "matches/live/team-talk?apply=false" @{}` before half-time → 400 "isn't on screen".
3. Dry run at `TeamTalk`: `Post "matches/live/team-talk?apply=false" @{}` → `lines` (6 lines in pt-BR, e.g. "Sigam o
   plano, mantenham a calma e vai ficar tudo bem.").
4. Apply: `Post "matches/live/team-talk?apply=true" @{ line = 0 }` → `said` is line 0, `reaction` lists the squad's
   reactions as the panel shows them.
5. Continue the match: press Continue in the game, or click `#continue-button` as `C:\Code\ht44.py` does (without its
   talk step).

## 5. Shouts

Not built: see the open question in the PR notes (the shout event's target and data weren't found without writing).

## Record

| Call | Time (s) | Result |
| --- | --- | --- |
| Live (baseline) | | |
| mentality dry / apply | | |
| substitution dry / apply | | |
| Live after the stoppage | | |
| role dry / apply | | |
| team-talk dry / apply | | |

Leave the save as it is: `/quit` doesn't save the test save.
