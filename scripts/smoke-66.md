# Smoke #66: captaincy (first PR)

Test save `MCPFM investigation` only, outside a live match. Before every write:
`curl.exe -s http://localhost:5121/session` must show `"isInvestigationSave": true`. If it doesn't, stop.
API at `http://localhost:5120` built from `agent/t66`. Run each case twice.

1. Read: `curl.exe -s http://localhost:5120/api/v1/setpieces`
   → note `captain`, `viceCaptain`, and `takerLists[Penalties].takers[0]`.
   In the test save on 17/11/2026: captain Neymar (19024412), vice Gabriel Barbosa (30035404),
   penalties Neymar, Gabriel Barbosa, Philippe Coutinho (19046041).
2. Dry run: `curl.exe -s -X POST -H "Content-Type: application/json" -d '{"captainId":19046041}' http://localhost:5120/api/v1/setpieces/captains`
   → `applied: false`, a step "Make Philippe Coutinho captain (now: Neymar)"; read again: captain still Neymar.
3. Apply: same POST with `?apply=true` → `after.captain` is Coutinho; read again shows Coutinho.
4. Back: `-d '{"captainId":19024412}'` with `?apply=true` → read shows Neymar again.
5. Edge, unknown player: `-d '{"captainId":999999999}'` → 404, read unchanged.
6. Edge, same player twice: `-d '{"captainId":19046041,"viceCaptainId":19046041}'` → 400, read unchanged.
7. Edge, menu doesn't offer it: `-d '{"viceCaptainId":30035404}'` while he's vice → note "already vice-captain", nothing written.
