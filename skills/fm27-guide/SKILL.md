---
name: fm27-guide
description: Football Manager 27 game knowledge and Copiloto conduct for Vini's Santos career, played through the football-manager MCP tools. Covers match and tactics, squad and training, market and board, and the facts of the Santos save, each rule marked confirmed in save or probable. Use when the user asks about FM 27 or the Santos save, before deciding a lineup, rotation, tactic, training plan, player talk, message reply, transfer, contract or board matter, and before writing anything to the game.
---

# FM 27 guide

The knowledge Claude checks before deciding in Vini's FM 27 career with Santos, and how Claude acts as **Copiloto**:
Vini drives the career and advances the calendar; Claude reads, decides, executes what he asks and explains.

## Confidence marks

Every rule in this guide carries one mark:

- *confirmed in save*: read in the save through the API or the plugin bridge. Trust it.
- *probable*: from research (Sports Interactive pages, community engine tests, guides) or deduction, not checked in
  the save. Use it, say so when a decision rests on it, and check it in the game when a cheap read can.

The save beats the research. When a read contradicts a rule, the read wins and the guide gets fixed (see Upkeep).

## How the Copiloto acts

1. **Read first.** Use the read tools (`get_squad`, `get_tactics`, `get_matches`, `get_team`, `get_training`,
   `get_inbox`, ...) for the facts a decision needs. Never invent attributes, ratings, dates or fees.
2. **Decide with this guide.** Load only the reference the request needs (index below). For a full tactic review
   or build, also follow the `fm-tactical-analysis` skill.
3. **Dry run every write.** Every writing tool runs dry by default; show the plan before `apply: true`.
4. **Warn before writing to the real save.** Right before an `apply` on Vini's career, tell him in one line:
   "não clique por 30 s" (don't click for 30 s), then write. Writes go to the real save only when Vini asked for them.
   The save guard copies the save and checks name, club and game date before the write; when it refuses, stop and
   report the reason, write nothing.
5. **One sentence of reason per decision.** Every pick, swap, fee or reply comes with one sentence saying why, so
   Vini can correct the judgement.
6. **At most one alert line.** If something important was not asked (serious injury, an offer received, a board
   ultimatum), add at most one line at the end of the answer. Mention it; take no action on it.
7. **Prefer the staff.** When a message offers "Delegar..." or "Pedir a..." for something the staff owns (press,
   friendlies, renewals, sales), prefer it. Tactics, lineup and target choice stay with Vini and Claude.

Copiloto is the only mode. Claude never advances the calendar or decides on its own between requests: Pilot mode is
out of scope.

Answer Vini in Brazilian Portuguese; quote the game's labels as the game writes them.

## Core rules

Match and tactics:

- Match fitness beats tactics: sharpness 75% costs about 0.37 points per game, condition 80% about 0.80. Rotation
  is the most valuable match decision. *probable*
- Start from a strong base style: Gegenpress or Fluid Counter-Attack led the engine tests. Park the Bus and
  Catenaccio were worst, even for weak sides. Both leaders exist in the save's style list. *confirmed in save* (styles
  exist), *probable* (ranking)
- No rock-paper-scissors: the stronger tactic wins almost every pairing. Keep the base tactic; adjust one or two
  instructions for a real problem. *probable*
- Mentality has 7 steps, `VeryDefensive` to `VeryAttacking`. One step more attacking cost points in tests; raise it
  only to chase a score. *confirmed in save* (steps), *probable* (effect)
- Tweaks such as Lock It Down and Goalkeeper Up For Set Pieces are for the last minutes, never from kick-off. *probable*

During a live match (`get_live_match`, read-only and light; `get_match` only sees the match after the final whistle):

- Read it at half time, right after a goal either way, and when Vini says a player is tiring. Say what changed in one
  line: score, xG and shots for and against, and the player most at risk. *confirmed in save* (out-of-match answer),
  *probable* (live values; the tool was built without a live match to test)
- Condition dropping under 60-65 on a starter, or a booked player on a low rating, is the cue to suggest a
  substitution; check `substitutionsRemaining` first. *probable*
- Never poll it in a loop or call `/ui`: heavy screen reads have frozen the live match. *confirmed in save*

Squad and training:

- Rotate 3 to 5 starters a match; a starter plays 2 games in 3. Out: injured, suspended, condition below 85. In:
  condition 92+, sharpness 75+ preferred; a merely tired starter only for a backup at most 2 rating points worse.
  *probable* (decided rules for the rotation tool)
- Pace and Acceleration weigh most in the engine for every outfield position; Jumping Reach for centre-backs and
  strikers; Agility and Reflexes for keepers. *probable*
- Praise training rated 7.5+, criticise 6.5 or less, once a month per player. The talk option disappears from the
  player's menu once used, even if the card closes unanswered. *confirmed in save* (option disappears),
  *probable* (thresholds)
- Keep promises small and kept; tell early who will not be renewed. *probable*

Market and board:

- Brazilian contracts end on 31 December; renew key players before their last 6 months. *confirmed in save*
- No financial fair play binds Santos in the save; the limits are the transfer and wage budgets. *confirmed in save*
- Bonuses sit outside the wage budget: add them to the real cost. *probable*
- Registration rules of Série A, Copa do Brasil and Sudamericana set no foreigner limit; read the next match's
  eligibility rule before each lineup anyway. *confirmed in save*
- Board goals marked Required are non-negotiable; financial goals are limits, not ambitions. *probable*

## References

Load one only when the request needs it:

| File | Read it for |
|---|---|
| [references/match-and-tactics.md](references/match-and-tactics.md) | Building or changing a tactic, instructions, tactical effectiveness, tweaks, shouts, team talks, set pieces, attributes the engine values, home advantage |
| [references/squad-and-training.md](references/squad-and-training.md) | Rotation, condition and sharpness, training plans, individual training, development by age, personality, morale, promises, talks, loans, youth intake |
| [references/market-and-board.md](references/market-and-board.md) | Scouting and targets, offers and counter-offers, clauses, contracts and wages, renewals, sales, board goals and requests, finances, Brazilian rules and calendar |
| [references/moneyball.md](references/moneyball.md) | Signing or selling by performance data (Allan FCL's moneyball): search flow, per-position cuts, pitfalls, sale rules |
| [references/santos.md](references/santos.md) | Facts of Vini's save: dates, stadium, contracts, competitions, staff, save files, game reads that work |

Research sources (with links) live only in the fork's branches `research/fm27-partida-tatica`,
`research/fm27-elenco-treino`, `research/fm27-mercado-diretoria` and `research/staff-delegation`
(`docs/research/*.md`).

## Upkeep

- When a read in the game confirms a *probable* rule, change its mark; when it refutes one, delete the rule and write
  the save's fact in its place (in `santos.md` when it is Santos-specific). Commit to the fork without review.
- After a big FM patch, re-check the *confirmed in save* rules before trusting them.
- Split this skill only when a Copiloto flow (e.g. signing a player) needs steps of its own.
