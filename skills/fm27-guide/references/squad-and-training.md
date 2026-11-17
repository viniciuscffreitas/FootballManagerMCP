# Squad and training

Rules for rotation, fitness, training, development, morale and the dressing room in FM 27. Marks: *confirmed in
save* = read in Vini's save; *probable* = research or deduction, not checked in the save. Numbers for sharpness and
attributes come from FM 26 community tests unless noted.

## Contents

1. Condition, fatigue and sharpness
2. Rotation
3. Availability before a match
4. Team training
5. Individual training and retraining
6. Attributes, hidden attributes, personality
7. CA, PA and development by age
8. Morale, hierarchy and buy-in
9. Promises and player talks
10. Young players and loans
11. Youth intake
12. Choosing roles by attributes
13. Weekly checklist

## 1. Condition, fatigue and sharpness

- Condition is today's energy. Fatigue builds up: a player can show a green heart and still be tired. Sharpness is
  the habit of playing. *probable*
- Sharpness weighs more than condition: 100% to 90% sharpness cut the win rate by about a third and raised injuries
  by about a quarter in one test. Tiredness is fixed by rotation; sharpness only by playing. *probable*
- A friendly gives sharpness at about 55% of the rate of a competitive match; gains slow near 100%. *probable*
- Natural Fitness speeds recovery between matches; Stamina holds condition during a match. *probable*
- `get_squad` reads condition and sharpness per player. *confirmed in save*
- The Medical Centre shows injury risk, 14-day match load, training load and sharpness; FM 27 training screens show
  this week's and next weeks' physical load. *probable*

## 2. Rotation

Rules of the rotation tool (decided with Vini): *probable*

- Out: injured, suspended, or condition below 85.
- In: condition 92 or more; sharpness 75 or more preferred.
- A starter who is only tired is swapped only for a backup rated at most 2 points lower.
- Running the rotation twice in a row changes nobody the second time.

General rules: *probable*

1. Squad of about 25, two per position, with three veteran mentors (one per line).
2. A starter plays 2 games in 3; the trusted backup plays the third plus 20 to 30 minutes in the others.
3. Rotate 3 to 5 per match; more breaks cohesion.
4. A backup plays a role he already knows.
5. Key matches (derbies with Corinthians, Palmeiras, São Paulo; direct rivals) get the rested starters; a cup tie
   against a small side gets backups and youngsters.
6. Never rush an injured player back: a relapse costs more than a match on the bench.
7. A star kept on the bench too long gets unhappy; meet the playing time agreed.
8. Train the backups in the same tactic: understanding and buy-in are per player, and an untrained backup lowers
   effectiveness.

Two-match weeks are common: 4 of 11 gaps between Paulistão matches were 3 or 4 days. *confirmed in save*

## 3. Availability before a match

- Injured: `CurrentInjury.InjuryAvailability` (1 = out). Suspended: `Club.MainTeam.AllCompsActiveBans` with
  competition, matches left and reason. Eligibility: `NextMatchEligibilityRule` and `NextMatchMaxSquadRuleStatus`.
  *confirmed in save*
- `UnavailableReason`, `IneligibilityReason` and `UnregisteredPlayers` stay pending in the bridge; do not rely on
  them. *confirmed in save*
- In a competition with registration rules (Paulistão), the Quick Pick takes only registered players; reapply the
  starters afterwards. Build the bench after the starters. *confirmed in save*

## 4. Team training

FM 27 changes: training is a main-menu area; weekly plans can be copied again; units are gone; first-team training
targets the tactic and the next opponent; the plan can be changed from the pre-match message; periodic training
reviews with quick praise or criticism are back. *probable*

Session blocks: *probable*

| Block | Sessions |
|---|---|
| Tactical Understanding | Passing Style, Build Up, Chance Creation, Pressing Intensity, Pressing Trap, Line of Engagement, In Possession Shape, Out of Possession Shape |
| Match Preparation | Defensive Block, Defensive Positioning, Defensive Engagement, Corners, Free-Kicks, Penalties |
| Footballing Development | Individual Development, Attacking Ball Work, Defensive Ball Work, Possession Ball Work |
| Physical & Recovery | Rest, Recovery, Light Conditioning, Conditioning, Heavy Conditioning |
| Extra-Curricular | Team Bonding, Community Outreach |

Building the week: *probable*

- Day after a match: Recovery or Rest. Peak load two days after, then taper to the next match.
- Eve of a match: Match Preparation against the opponent's strength; set pieces if he is strong at them.
- Free week: Tactical Understanding sessions of your tactic plus Individual Development.
- Three-match week: almost only Recovery, Rest and light preparation.
- Falling morale: Team Bonding or Community Outreach.
- Keep each coaching category's workload Light or Average; heavy workload lowers training quality.
- Review a copied plan when the calendar changes; never copy a one-match week into a three-match week.
- Train up to 3 tactics; the alternatives stay less familiar than the main one.

`get_training` reads this week's and next week's sessions and each player's rating; `set_training` changes sessions,
additional focus and intensity; both work in the save. *confirmed in save*

## 5. Individual training and retraining

- Each player trains an IP and an OOP position and role, plus an additional focus. FM 27 adds the focuses Chance
  Conversion, Chance Creation, Ball Retention, Ball Progression, Pressing and Containment. *probable*
- Intensity: leave Automatic for most; Double only for a healthy youngster with high Natural Fitness; Half or rest for
  high injury risk. *probable*
- Praise training rated 7.5 or more, criticise 6.5 or less, once a month per player. *probable*
- Position scale: Natural 18-20, Accomplished 15-17, Competent 12-14, Unconvincing 9-11. Aim for 12+. A new position
  from scratch takes 6 to 9 months to be useful and about 12 to be Natural; playing there and high Versatility speed
  it up; the young learn faster. *probable*

## 6. Attributes, hidden attributes, personality

- Visible attributes run 1-20 (technical, mental, physical; set pieces now separate). *probable*
- Pace and Acceleration weigh most in the engine (see match-and-tactics.md, section 10). *probable*
- Hidden attributes: Professionalism (most important for development), Ambition (faster growth, wants a bigger move),
  Pressure (penalty taker, captain), Important Matches (finals, derbies), Consistency, Injury Proneness, Versatility,
  Adaptability (foreign signings), Loyalty, Temperament, Sportsmanship, Controversy. Of Consistency, Important Matches
  and Pressure, Pressure weighs most. *probable*
- `get_squad` hides them by default; read them only when Vini agreed in the conversation. *confirmed in save*
- Personality comes from Determination, Leadership and six hidden attributes. Best for development: Model Citizen,
  Model Professional, Perfectionist, Professional, Resolute. Avoid for an expensive youngster: Slack, Casual,
  Temperamental, Low Self-Belief, Easily Discouraged. Watch Mercenary, Fickle, Very Ambitious: they grow but force
  moves. *probable*

## 7. CA, PA and development by age

- CA is current ability, PA the ceiling, both 0-200. PA says how far a player can go, not how far he will: personality,
  minutes, training and injuries decide. Stars are relative to your squad and league and to the judging staff. *probable*
- Growth by age: 16-19 a lot, 20-23 well, 24-26 a little, 27+ almost none; physicals start to fall near 30. Physicals
  grow first, mentals last. *probable*
- Never promise "he will be a star". Say "high ceiling according to staff X" and cite the personality. *probable*

## 8. Morale, hierarchy and buy-in

- Morale feeds dynamics, cohesion, training, form and preparation. *probable*
- Hierarchy tiers: Team Leaders, Highly Influential, Influential, Other. Keep the top happy first; crossing a member
  of the core social group turns the group against you. *probable*
- In FM 27 hierarchy and social groups feed tactical buy-in: a split dressing room weakens the tactic. *probable*
- Raising morale: happy captain and leaders, solve the unhappy, team meeting on goals, praise in the press, Team
  Bonding, coaches with high Motivating. Few team meetings; no harsh criticism in the press. *probable*
- Signings: personality and fit with the core group count as much as attributes. *probable*

## 9. Promises and player talks

- Playing time is the heart of happiness. Broken promises, including those made in contract talks, cost relationship
  and harmony. *probable*
- Before promising minutes, count the starters at that position; never promise the same place to two players; record
  each promise and its deadline and remind Vini before it falls due. *probable*
- Tell early who will not be renewed. A big-name pre-contract six months before his deal ends lifts morale. *probable*
- Players differ: some want support, some public praise, some criticism. *probable*
- A talk opens from the player's context menu (praise or criticise training, performance, conduct). Opening and
  closing it unanswered still spends it: the option vanishes from the menu. *confirmed in save*
- Squad meetings (e.g. expectations and promises) are mandatory and block Continue until answered. *confirmed in save*
- An FM 26 bug showed "broken promise" with an empty list; if it appears, tell Vini it may be a bug. *probable*

## 10. Young players and loans

- 15-17: physical and line focus, not an exact role. 18-21: fix technical gaps, set a role, start loans. 21+: fight
  for a place or be sold; at 21 a youngster is often worth more than his ability. *probable*
- Up to 18 a player grows mostly through training; after, through matches and ratings (15 minutes earn a rating).
  Target for a promoted youngster: 900 minutes or 10-20 matches a season, coming on in won games. Promote only who will
  play. *probable*
- Mentoring changes personality, not attributes: pair the youngster with veterans of good personality, groups of 3+.
  *probable*
- Loan players aged 18-24 who will not play; above 24, prefer selling. The destination needs a starting place, good
  facilities, the right level (equal to or a little above the coach's "level of performance") and his natural
  position. Always ask for a recall clause and agreed playing time. *probable*
- For Santos: Série B and mid-table Série A clubs usually give minutes; Europe only for those already at that level.
  *probable*

## 11. Youth intake

- Santos' intake date in the save is 14/01/2027 (`Club.IntakeDate`). *confirmed in save*
- The preview arrives about 3 months before and may not match the result. *probable*
- Country youth rating and football importance are fixed (Brazil is high). Raise Youth Facilities, Junior Coaching and
  Youth Recruitment only through board requests. *probable*
- The Head of Youth Development shapes the intake's type through his preferred formation and passes his personality
  on. Prefer one with your formation and a Model Citizen, Perfectionist, Model Professional or Professional
  personality. Check him and free under-20 places about three months before the intake. *probable*

## 12. Choosing roles by attributes

1. Run `get_role_fit` at the position: the game rates every player in every role. *confirmed in save*
2. Check the role's key attributes; aim for 12+ in Santos' league. *probable*
3. Break ties with Pace and Acceleration on the flanks, at full-back and up front. *probable*
4. Prefer a natural position in both phases to a "better" role out of position. *probable*
5. Traits that match the role help; traits that contradict it hurt. *probable*

The OOP role needs its own attributes: a central midfielder can suit Channel Midfielder IP and fail as Screening
Defensive Midfielder OOP. *probable*

## 13. Weekly checklist

1. `get_squad`: condition, sharpness, morale, who plays little.
2. Calendar: 1, 2 or 3 matches this week? Adjust the plan.
3. `get_training`: load, training ratings, individual intensity.
4. `get_tactics`: the four effectiveness parts; work on the weakest.
5. `get_team` for the opponent: pick the Match Preparation session.
6. Availability, then the lineup with 3-5 rotations; flag who needs sharpness.
7. Promises and contracts falling due.
