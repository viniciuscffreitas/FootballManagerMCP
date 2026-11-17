# Match and tactics

Rules for building, changing and running a tactic in FM 27. Marks: *confirmed in save* = read in Vini's save;
*probable* = research or deduction, not checked in the save. Engine numbers come from community tools that run the
FM 27 match engine outside the game, on synthetic English leagues; an engine patch was due the week of 16/11/2026 and
may move them. Points per game = ppg.

## Contents

1. What changed from FM 26
2. Building a tactic
3. Styles and mentality
4. Team instructions
5. Tactical effectiveness and complexity
6. During the match: tweaks, shouts, substitutions
7. Team talks
8. Opposition instructions
9. Set pieces
10. Attributes the engine values
11. Context outside the tactic screen
12. Common mistakes

## 1. What changed from FM 26

- Two formations per tactic, in possession (IP) and out of possession (OOP), as in FM 26. *probable*
- Roles are split IP and OOP, with no duty (Defend/Support/Attack). FM 27 adds player instructions for box
  attacking: late arrival or attacking the line, near post, far post or centre. *probable*
- Tactical Familiarity became **Tactical Effectiveness** (four parts) plus **Tactical Complexity** (section 5).
  The API reads effectiveness in `get_tactics`. *confirmed in save*
- **Tactical Tweaks** change the tactic in a match without the tactics screen (section 6). *probable*
- Shouts were rebuilt to act on mental state. *probable*
- Training units are gone; team training prepares the next match (see squad-and-training.md). *probable*
- Engine: pace and acceleration rebuilt from tracking data; Marking, Anticipation and Stamina weigh more. *probable*

## 2. Building a tactic

Order: style, IP formation, OOP formation (the game suggests three that fit the IP one), an IP and an OOP role per
player, then team instructions per phase and third. *probable*

- The farther a player walks between the two formations, the more holes open. Small changes are enough, e.g. in a
  4-2-3-1 drop the wide attacking midfielders to the midfield line OOP. *probable*
- Think of the rest defence (who stays back when attacking) and the rest attack (who stays up for the counter). *probable*
- Pair roles: a player who carries the ball IP fits a pressing role OOP; a holding midfielder fits a screening role.
  *probable*
- Playing 3 or 4 players out of position only in the OOP shape had no measured cost, but natural positions in both
  phases raise effectiveness. Prefer natural positions. *probable*
- OOP role families: Pressing, Stopping, Covering, Screening, Tracking, Outlet, Dropping, Wide Cover. Mix pressers,
  screeners and trackers; one flank tracks while the other stays as an outlet. Outlet roles suit counter-attacking
  styles. *probable*
- Role ratings and keys come from `get_role_fit` and `get_tactic_options`; `set_tactic` writes. *confirmed in save*
- A tactic in an empty slot needs the 3-step creator (style, IP formation, OOP formation). *confirmed in save*

## 3. Styles and mentality

- `get_tactic_options` lists 11 styles in the save, Gegenpress and Fluid Counter-Attack among them. Use its keys,
  not names from research. *confirmed in save*
- Neutral-league test, same squad for all: Gegenpress 1.39 ppg, Fluid Counter-Attack 1.38, Wing Play 1.32, Control
  Possession 1.28, Route One 1.21, Direct Counter-Attack 1.20, Vertical Tiki-Taka 1.19, Tiki-Taka 1.15,
  Catenaccio 1.03, Park the Bus 0.92. Gaps under 0.1 ppg are noise. *probable*
- A season with each style: Gegenpress, Control Possession and Fluid Counter-Attack were the top three for a
  favourite, a bottom side and a fifth-tier side. The spread was larger in weaker leagues. *probable*
- Parking the bus does not save a weak team; it was worst for the weakest side too. *probable*
- No rock-paper-scissors: in 306 head-to-head pairs, overall tactic strength predicted all but one. Keep the base
  tactic against any opponent. *probable*
- Mentality keys: `VeryDefensive`, `Defensive`, `Cautious`, `Balanced`, `Positive`, `Attacking`, `VeryAttacking`.
  *confirmed in save*
- One step more attacking cost 0.12 ppg and 0.22 xG difference; one or two steps more defensive changed nothing
  clear. Raise mentality only to chase a score. *probable*

## 4. Team instructions

- IP groups: overview, build-up, progression, final third (attacking transition lives here). OOP groups: overview,
  high press, mid block, low block (defensive transition lives here). *probable*
- In 125 isolated changes on a working tactic, almost none moved results. Change one or two instructions to fix a
  problem seen in matches; never rebuild after one defeat. *probable*
- Leave the pressing trap on default: "show outside" cost 0.23 ppg, "show inside" 0.19. *probable*
- High tempo and frequent pressing burn more energy. *probable*

Instructions that pull against each other (guide heuristics): *probable*

| Bad pair | Problem | Fix |
|---|---|---|
| Direct passing + work ball into box | One rushes, one waits | Direct with low patience, or short with work into box |
| High press + low defensive line | Gap between midfield and defence | High press with high line; low block with low line |
| Counter-press + low block + regroup | Opposite orders | Counter-press with high press |
| Play out of pressure + long goal kicks | The build-up vanishes at every goal kick | Short goal kicks |
| More expressive + weak technicians | Constant turnovers | Balanced |

## 5. Tactical effectiveness and complexity

Four parts: *confirmed in save* (`get_tactics` reads them), *probable* (how to raise each)

| Part | Measures | Raise it with |
|---|---|---|
| Tactical understanding | Do the players know the system | Training time on its areas, good coaches, a stable identity |
| Player ability | Attributes and position to execute | Natural positions in both phases, few instructions early |
| Buy-in | Will to follow the manager | Praise good performers, a happy hierarchy, signings who fit the group |
| Next-match information | Knowledge of the opponent | Analysts, opposition reports |

- Understanding from 20 to 80 is worth about +0.52 ppg (like +7 Pace for the whole squad). Most of the gain is there
  by about 50; 90 and 100 play the same. *probable*
- Understanding has eight areas (IP shape, passing style, build-up, chance creation, OOP shape, line of engagement,
  pressing intensity, pressing trap). Raising one alone barely helps: the tactic is learned as a whole. *probable*
- Training counts double a match for understanding. *probable*
- One in-game test: effectiveness 30% to 70% took wins from 50% to 63% over five seasons. Understanding may decay
  fast each week at launch, a likely patch target. *probable*
- Complexity counts team, player and opposition instructions. Simple plans work fast with a low ceiling; complex
  ones learn slowly with a high ceiling. Against a little-known opponent, keep it simple. *probable*
- Keep one main tactic and at most one variation with the same shape; train it into the green, not to 100%.
  *probable*

## 6. During the match

Tweaks: *probable*

- Each tweak is a bundle (instructions one step, player instructions, sometimes mentality), stays on until switched
  off, unlimited. Names in game data: Lock It Down, Throw The Kitchen Sink, Play With More Patience, Play With More
  Urgency, Make Something Happen, Go Narrower, Exploit The Width, Go Long, Play Out The Back, Disrupt, Press
  Aggressively, Stick To The Plan, Swap Wings, Goalkeeper Up For Set Pieces.
- Whole-match tests: Lock It Down about -0.2 ppg; Goalkeeper Up For Set Pieces +0.22 to +0.28 goals conceded per
  game; Kitchen Sink, Go Long and More Urgency neutral on a middling tactic and harmful on one already at the extreme.
  No tweak helped on average. Late-game use after a goal was not tested.

Shouts: rebuilt in FM 27 to act on mental state, with context and staff feedback. No official list or measured
effect. Old practice: encourage when level or one down, calm after scoring first, praise when two up, demand more
when the team does not react. *probable*

Substitutions: Stamina weighs more on in-match condition, and benches matter more. At 70% condition a player loses
almost 3 points of Pace, Acceleration, Decisions and Positioning. Replace heavy pressers early. *probable*

Changing the lineup in a live match makes no substitution; substitutions and shouts have no tool and stay with
Vini. *confirmed in save*

## 7. Team talks

- Morale at kick-off barely changes the result (morale 5, 10 or 20 within ±0.04 ppg); it moves attributes by under
  a point, mostly Flair. Treat the talk as insurance against damage, not a source of points. *probable*
- Morale still matters over a season through training, form and happiness. Praise of good performance raises buy-in.
  *probable*
- Aggressive talk when winning breeds complacency; passive talk when losing reads as giving up; vary the message.
  *probable*
- The talk can be delegated to the assistant (see market-and-board.md, Staff). *probable*

## 8. Opposition instructions

- Target 1 to 3 dangerous players only. Inverted winger: show onto the weak foot and press. One-footed player: show
  onto the weak foot, unless he is training it. Tight marking suits a slow creator, not a much faster striker.
  *probable*
- They add complexity. Switching them off showed no measured effect. *probable*

## 9. Set pieces

- Jumping Reach decides aerial play (about 3.3 cm of jump per point); it is worth far more for centre-backs and
  strikers than for midfielders. Three strong jumpers bring most of the aerial gain. *probable*
- Corner and cross takers: Corners, Crossing, Technique. Direct free kicks: Free Kick Taking (plus Long Shots from
  distance). Penalties: Penalty Taking. *probable*
- Never send the keeper up except in the final desperate moments. *probable*

## 10. Attributes the engine values

Expected points per game per attribute point, whole squad: *probable*

| Attribute | Per point | Note |
|---|---|---|
| Acceleration | +0.068 | Worth most below 11 |
| Pace | +0.062 | Worth most below 13 |
| Jumping Reach | +0.035 | Worth most above 9 |
| Stamina | +0.034 | Very bad below 4 |
| Anticipation | +0.024 | Linear |
| Work Rate | +0.022 | Only matters below 9 |
| Concentration | +0.021 | Linear |
| Passing, Decisions, Technique, Tackling, Marking, Positioning | near zero | |

- Finishing and Strength do not show in xG but turn chances into goals: Finishing 1 to 20 is worth about 0.3 goals
  per game. *probable*
- Keepers: Agility and Reflexes dominate; each level saves 0.8 to 0.9 goals a season. A keeper rated 10 at the
  position costs about 0.53 ppg. *probable*
- Current Ability is not used in the match; the engine reads attributes. *probable*
- On a tie between two options, pick the faster one (less so for a keeper or a target striker). *probable*

## 11. Context outside the tactic screen

| Situation | Effect per game |
|---|---|
| Match sharpness 50% | -1.03 points |
| Match sharpness 75% | -0.37 points |
| Condition 80% | -0.80 points |
| Condition 90% | -0.28 points |
| Team cohesion, minimum to maximum | +0.2 to +0.3 points |
| Playing at home | +0.37 points, +0.22 xG |
| Kick-off morale | no measured effect |

All *probable*. Home advantage: grounds of 40,000+ give more than double the edge (+0.39 xG); smaller grounds give
+0.15 to +0.18 xG with no trend between 4,000 and 40,000. Vila Belmiro holds 16,068 (*confirmed in save*), so expect
the ordinary home edge. *probable*

## 12. Common mistakes

1. Rebuilding the whole tactic after a defeat.
2. Picking players without sharpness or tired.
3. IP and OOP shapes far apart.
4. High press with a low line, or with a squad short of stamina.
5. Moving the pressing trap inside or outside.
6. Raising mentality by default.
7. Lock It Down too early, or the keeper up without need.
8. Piling up opposition and player instructions.
9. "More expressive" with poor technicians.
10. Copying an internet tactic without checking the squad has its attributes.

All *probable*, from sections above.
