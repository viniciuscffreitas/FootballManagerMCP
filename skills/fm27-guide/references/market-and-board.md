# Market and board

Rules for scouting, transfers, contracts, finances, the board, the staff and Brazilian football in FM 27. Marks:
*confirmed in save* = read in Vini's save; *probable* = research or deduction, not checked in the save. When a
competition rule matters, read it in the save (registration rules, transfer window) before trusting this file.

## Contents

1. What changed in FM 27
2. Scouting and targets
3. Offers and counter-offers
4. Clauses
5. Contracts and wages
6. Renewals and pre-contracts
7. Sales
8. Staff responsibilities
9. Board: goals, confidence, requests
10. Finances
11. Brazilian rules and calendar
12. Checklist

## 1. What changed in FM 27

- Agent offers stay active on the Recruitment screen for the whole window and grow near its end. *probable*
- Deadline Day has its own screen again. *probable*
- Young players weigh playing time much more when choosing a club; the "stepping stone" promise counts more. *probable*
- A third club's sell-on percentage shows in the scout report, and the selling club asks more for that player. *probable*
- The Director of Football advises on transfer and loan fees and payment structure, from scouting to live talks.
  *probable*
- AI target lists weigh experience and dressing-room leadership more. *probable*
- The monthly report shows finances, loanees and the board's and fans' ratings. *probable*
- FM 27 says nothing new about the board, club vision or requests; FM 26 rules apply. *probable*

## 2. Scouting and targets

- Recruitment holds scouting, transfers, contracts and staff. Read Recruitment Objectives (board and fan
  expectations) before proposing signings. *probable*
- Requirements (TransferRoom In) advertise a need to all clubs, who answer with players; Pitch Opportunities
  (TransferRoom Out) show what others want. The competition's Team of the Week is a quick source of names. *probable*
- Before recommending a target, check the scout report, injury history, contract and the role he expects. Need comes
  before want: a backup striker before a flashy winger. *probable*
- A club far below its predicted place sells to reinvest; a club facing relegation takes a worse deal. *probable*
- Value Pace and Acceleration everywhere, Jumping Reach at centre-back and striker, Agility and Reflexes in goal (see
  match-and-tactics.md, section 10). *probable*
- Tools: `run_player_search`, `get_competition_stats`, `get_scouted_players`, `scout_player`, `shortlist_player`.
  Shortlisting to the default list works in the save; creating a shortlist needs its screen open. *confirmed in save*

## 3. Offers and counter-offers

- Ask the agent about availability first: expected wage, rough fee, interest. *probable*
- An offer has a fee up front, instalments, add-ons (appearances, goals) and clauses (sell-on, buy-back). *probable*
- Clubs counter more since FM 26. Do not take the first answer; change the structure, not only the total; drop
  needless clauses; instalments often close without raising the price. *probable*
- Asking prices and wages are higher early in the window and fall late. *probable*
- Instalments are debt: three signings of 5 million paid in instalments commit 15 million ahead. *probable*
- Loans: an optional purchase is low risk; an obligation is a delayed purchase; a wage split controls the cost.
  *probable*
- In the save the offer panel opens with the DoF's suggested fee; the fee moves by R$ 10 mil per click; counter-offers
  arrive as messages; an accepted fee leads to the contract panel. *confirmed in save*

## 4. Clauses

- A release clause forces the club to let the player negotiate. Find them with Contract > Clauses (Release) in the
  search or a column on a club's squad screen. *probable*
- Relegation and non-promotion release clauses do not apply in the mid-season window. *probable*
- Selling existing clauses gives quick cash; it is a patch, not a strategy. *probable*
- When renewing your own players, remove or raise the release clause before the player's value climbs; a clause below
  market value draws rivals. *probable*

## 5. Contracts and wages

- Parts: wage, signing-on fee, agent fee, squad status, length. *probable*
- Bonuses sit outside the wage budget: only Player Wages count, and the game does not warn. A yearly 5% rise also adds
  up. Add bonuses to the real cost; for starters and scorers prefer a slightly higher base and fewer appearance and
  goal bonuses. *probable*
- Set a wage scale by role (key player, starter, rotation, prospect); never pay a starter's wage to a bench player.
  Someone paid far above the squad triggers parity demands. *probable*
- The promised squad status must match the squad or the deal falls through; a broken promise breeds unhappiness.
  *probable*
- Offer young players real minutes. Free agents ask for high signing-on fees, agent fees and wages. *probable*

## 6. Renewals and pre-contracts

- Brazilian contracts end on 31 December: 33 of 40 players in the save. *confirmed in save*
- In the last 6 months a player may talk to other clubs, so the pre-contract window opens around July. Renew key
  players before then. *probable*
- Recruitment highlights contracts with under 12 months left. *probable*
- Tie the best down long; tell early who will not be renewed; better deals for leaders help morale. *probable*
- Player contract talks go to the DoF by default (section 8). *probable*

## 7. Sales

- Sell before the last months of a contract, while value remains. *probable*
- Offer surplus players through Pitch Opportunities (right-click in the squad). An intermediary to push a player out
  is a last resort that may upset him. *probable*
- When selling a promising youngster, ask for a sell-on or buy-back: the AI pays more for players with a sell-on.
  *probable*
- Before any big sale ask: does the club still afford this if it misses its main goal? *probable*
- The European summer window (July to August/September) brings most offers for Santos' youngsters; plan the
  replacement first. *probable*

## 8. Staff responsibilities

- Delegable: press conferences and tunnel interviews, team talks, shouts, friendlies (running and arranging),
  tactical briefing and opposition instructions, set pieces, training, scouting, staff hiring and courses, youth
  teams, sales and loans (DoF), player contract talks (DoF by default). *probable*
- Transfers split into initiate, negotiate and finalise. If the DoF holds negotiate and finalise, he finishes what the
  manager started. The DoF only chases players marked as targets; target urgency sets how hard he bids. *probable*
- Stays with the manager: tactics and substitutions in competitive matches (only Go On Holiday hands them to the
  assistant), the board, and choosing targets. *probable*
- In FM 27 "delegate responsibilities" sits on the control panel of the Squad and Recruitment screens; check the exact
  labels in the game. *probable*
- Messages that block Continue often offer "Delegar para ..." or "Pedir a ..."; prefer them for staff matters. A
  transfer offer for a squad player is mandatory with Accept, Reject, View, Delegate: delegate to the DoF, never accept
  by default. *confirmed in save*

## 9. Board: goals, confidence, requests

Club philosophy = board vision + season goals + financial rules + youth goals + fan expectations; when board and fans
disagree, the board wins. *probable*

| Level | Weight | Example |
|---|---|---|
| Required | Non-negotiable; failing cuts confidence at once; repeating brings an ultimatum or the sack | League position, wage control |
| Desired | Ambition; failing hurts only with other bad results | Title challenge |
| Preferred | How the board likes to work; little loss if ignored | Signing policy, cup beyond the minimum |
| Favoured | Identity; almost no penalty | Youth, style |

All *probable*:

1. Financial goals are limits, not ambitions; breaking the wage budget cuts confidence at once.
2. Style goals are judged by output (chances, goals, control), not by the formation.
3. Negotiate goals when the vision is set; use "Don't Judge" on the competition you will sacrifice.
4. Early matches, press conferences and media answers weigh on confidence.
5. Board requests (Club > Board): facilities, budgets, affiliates, B team, youth recruitment, contract, coaching
   course, stadium, staff. Ask when finances are stable and the team is above expectation; few requests before you
   are established. A refusal may come with a smaller offer; the wait to ask again is random.

## 10. Finances

- Three numbers: transfer budget (one-off), wage budget (recurring), balance. The wage budget weighs more on long-term
  health. The split between them is adjusted on the Recruitment overview. *probable*
- Before each window review balance, income, costs and future payments; keep a reserve for injuries. *probable*
- No financial fair play binds Santos in the save: `FfpRequirementsCount` 0, `FfpData` null, `CanViewFinancesFFP`
  false. The real-world CBF sustainability system and the FM 27 Squad Cost Ratio do not apply here. *confirmed in save*
- Santos' budgets: read them in the save; the research has no number. *probable*

## 11. Brazilian rules and calendar

Foreign players:

- The registration rules of Série A, Copa do Brasil and Sudamericana limit only squad size and goalkeepers, not
  foreigners. *confirmed in save*
- A per-match limit never showed (`NextMatchEligibilityRule` empty with 6 foreigners); read it before every match.
  *confirmed in save*
- Real-world limits (9 per match in 2026, falling to 5 by 2030) may arrive with a database update; prefer Brazilians
  or Brazilian passport holders on long deals. *probable*

Transfer windows:

- The Brazilian window was closed on 12/03/2026 in the save (open: Argentina, France, Peru). Read
  `TransferWindowOpenCompetitions` instead of assuming dates. *confirmed in save*
- Real-world 2026 windows were 5/jan-3/mar and 20/jul-11/set; 2027: 4/jan-2/mar and 19/jul-10/set. *probable*
- A free agent can sign outside the window but plays only when registration allows. *probable*
- Real-world Série A rule: a player who played 12+ matches for one Série A club cannot play for another that season;
  whether the game models it is unknown. *probable*

Calendar and competitions:

- 2026 in the save: only the Paulistão from 11/01 to 08/03; the Brasileirão starts on 15/03; two-match weeks are
  common. *confirmed in save*
- Santos is registered in the CONMEBOL Sudamericana 2026. *confirmed in save*
- Read the fixtures with `get_matches` rather than real-world calendars; the game's 2027 dates are unchecked.
  *probable*
- Paulistão: a short league phase (8 matches) then knock-outs, with relegation; a big club can go down. Never field
  only reserves without checking the table. *probable*
- Brasileirão: 20 clubs, 38 rounds, 4 relegated; top 4 straight to the Libertadores group stage, 5th to the
  qualifiers, Sudamericana places below. The Copa do Brasil winner reaches the Libertadores group stage, the runner-up
  the qualifiers: the cup is Santos' shortest road to the Libertadores and pays by round. *probable*
- In FM 27 South American continental matches no longer fall on Fridays. *probable*

## 12. Checklist

1. Read the board goals; mark what is Required; suggest "Don't Judge" where the team will rotate.
2. Read the competition rules in the save: eligibility, registration, window.
3. Before each window: balance, wage bill, future payments, contracts with under 12 months.
4. For each target: ask the agent, read the report (third-party sell-on), confirm the promised role.
5. Negotiate the structure: instalments, add-ons, clauses; add bonuses to the real cost.
6. Promise young players only minutes you will give.
7. Renew key players before their last 6 months; move the release clause when renewing.
8. Sell before the last year; ask for a sell-on or buy-back on youngsters.
9. Board requests only with stable finances and the team above target.
