# Smoke #66: captaincy (first PR)

Test save `MCPFM investigation` only, outside a live match. Before every write:
`curl.exe -s http://localhost:5121/session` must show `"isInvestigationSave": true`. If it doesn't, stop.
API at `http://localhost:5120` built from `agent/t66`. Run each case twice.

1. Read: `curl.exe -s http://localhost:5120/api/v1/setpieces`
   → note `captain`, `viceCaptain`, and `takerLists[Penalties].takers[0]`. Every side list (`CornersLeft`, ...) has
   `sideVerified: false`; Penalties has none.
   In the test save on 17/11/2026: captain Neymar (19024412), vice Gabriel Barbosa (30035404),
   penalties Neymar, Gabriel Barbosa, Philippe Coutinho (19046041).
2. Dry run: `curl.exe -s -X POST -H "Content-Type: application/json" -d '{"captainId":19046041}' http://localhost:5120/api/v1/setpieces/captains`
   → `applied: "NotWritten"`, a step "Make Philippe Coutinho captain (now: Neymar)"; read again: captain still Neymar.
3. Apply: same POST with `?apply=true` → `applied: "Written"`, `captainWritten: "Written"`, `confirmed: true`,
   `after.captain` is Coutinho; read again shows Coutinho.
4. Back: `-d '{"captainId":19024412}'` with `?apply=true` → read shows Neymar again.
5. Edge, unknown player: `-d '{"captainId":999999999}'` → 404, read unchanged.
6. Edge, same player twice: `-d '{"captainId":19046041,"viceCaptainId":19046041}'` → 400, read unchanged.
7. Edge, already in the role: `-d '{"viceCaptainId":30035404}'` while he's vice → note "already vice-captain", nothing written.
8. Vice swap: `-d '{"viceCaptainId":19046041}'` with `?apply=true` → `viceCaptainWritten: "Written"`, `confirmed: true`,
   `after.viceCaptain` is Coutinho; read shows vice Coutinho and captain still Neymar.
   Back: `-d '{"viceCaptainId":30035404}'` with `?apply=true` → `confirmed: true`; read shows vice Gabriel Barbosa.
9. Edge, the captain made vice without a new captain (refused in the plan, before the save guard and the menu):
   dry run `-d '{"viceCaptainId":19024412}'` (no `apply`) → 400 "Neymar is the captain ... Name a new captain too".
   Then the same with `?apply=true` → 400 with the same reason; read unchanged (captain Neymar, vice Gabriel Barbosa).

Pass only when every read after a write shows the change (or no change for the edge cases); HTTP 200 alone is not a pass.
After the run, read once more: captain Neymar, vice Gabriel Barbosa, as at the start.
