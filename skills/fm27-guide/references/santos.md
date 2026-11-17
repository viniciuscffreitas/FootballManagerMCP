# Santos save

Facts of Vini's FM 27 career with Santos. All *confirmed in save* unless marked *probable*. Most were read on
17/11/2026 in the test save `MCPFM investigation.fm` at game date 12/03/2026; the test save is a copy of the same
career, so club facts carry over, but dated facts (date, window, squad) move on in the real save: re-read them.

## Contents

1. Saves and the game
2. Club
3. Squad and contracts
4. Competitions and calendar
5. Market and finances
6. Staff
7. Game reads that work

## 1. Saves and the game

- FM 27 from Game Pass on Vini's Windows PC; game 27.0.1.2494769, package 0.9.2435.0 on 17/11/2026. A new package
  version blocks writes to the real save until a short test on the test save releases it.
- Real save: Vini's Santos career. Only the Copiloto writes to it, on Vini's request, after the "não clique por 30 s"
  warning and a backup.
- Backups in `C:\Code\fm-backup`, named `<Club>-<YYYYMMDD-HHMM>-jogo-<YYYY-MM-DD>.fm`; only the two newest Claude
  backups are kept. `Santos-20261116-pre-mcpfm.fm` (before MCPFM) is never deleted, nor files outside the pattern.
- Test save: `MCPFM investigation.fm`. Every experiment happens there, never in the real save. `/quit` closes it
  without saving.
- In-game language: Portuguese (Brazil). Labels and message options come in Portuguese.

## 2. Club

- Stadium: Vila Belmiro, capacity 16,068 (`Club.MainTeam.Stadium.TotalCapacity`). Home edge as any ground under
  40,000. *probable* (effect)
- Youth intake: 14/01/2027 (`Club.IntakeDate`), 308 days after 12/03/2026. Not September or October.
- Main derbies: Corinthians, Palmeiras, São Paulo. *probable* (by football knowledge, not read)

## 3. Squad and contracts

- 40 players read; 33 contracts end on 31/12 (`FullContract.ExpiryDate`).
- 6 foreign players; no per-match foreigner rule appeared (`NextMatchEligibilityRule` empty).

## 4. Competitions and calendar

- 2026 competitions: Paulistão, Brasileirão Série A, Copa do Brasil, CONMEBOL Sudamericana 2026
  (`SquadRegistrationComps`).
- Registration rules of Série A, Copa do Brasil and Sudamericana: squad size and goalkeepers only.
- Paulistão alone from 11/01 to 08/03/2026; Brasileirão from 15/03/2026.
- 4 of 11 gaps between Paulistão matches were 3 or 4 days: two-match weeks are normal.
- Paulistão has a registration rule: Quick Pick takes only registered players.

## 5. Market and finances

- No financial fair play: `FfpRequirementsCount` 0, `FfpData` null, `CanViewFinancesFFP` false.
- On 12/03/2026 the Brazilian window was closed; open windows: Argentina, France, Peru
  (`TransferWindowOpenCompetitions`).
- Offer fee steps of R$ 10 mil per click in the offer panel.
- Transfer and wage budgets: not recorded yet; read them before any market advice.

## 6. Staff

- Director of Football: Alexandre Mattos (the inbox offers "Delegar para Alexandre Mattos" on transfer offers).
- Assistant: Cuquinha (the Code of Conduct message offers "Pedir a Cuquinha para escolher").
- Delegation preferences (which staff owns press, friendlies, renewals): not read yet. *probable* that defaults apply.

## 7. Game reads that work

Useful bridge paths (read-only):

| What | Read |
|---|---|
| Game date | `/get?root=game&props=CurrentGameDate` (ISO) |
| Injury | `Club.MainTeam.Players[i].CurrentInjury.InjuryAvailability` (1 = out) |
| Suspensions | `Club.MainTeam.AllCompsActiveBans` (player, BanScope, ActiveBanMatchesRemaining, ActiveBanReason) |
| Next match rules | `Club.MainTeam.NextMatchEligibilityRule`, `NextMatchMaxSquadRuleStatus` |
| Registration | `Club.MainTeam.SquadRegistrationComps[i]` (SquadSelectionName, SquadSelectionRules) |
| Contract end | `Players[i].FullContract.ExpiryDate` |
| Window | `Players[i].TransferWindowData.TransferWindowOpenCompetitions` |
| Required replies | `ManagerResponseRequiredCount` (root Human) |
| Message options | `SpeakTo.Actions` of `HumanPortalFilteredMessages[UniqueId=<id>]` |

`UnavailableReason`, `IneligibilityReason` and `UnregisteredPlayers` stay pending: do not use them.
