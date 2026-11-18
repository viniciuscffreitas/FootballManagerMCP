"""Smoke of ticket #71 (contracts from the player's menu) against the API on the Windows PC.

Run on the Windows PC, with the test save 'MCPFM investigation' loaded and no match being played:
    python smoke-71.py menus <playerId>...           # read only: what each contract action's menu offers him
    python smoke-71.py status <playerId> <status> <original>  # x2: apply <status>, then back to <original> (menu labels)
    python smoke-71.py gone <playerId>               # he's no longer in get_squad
    python smoke-71.py dof <playerId>                # dry run, apply, then the menu no longer offers it (x2)
    python smoke-71.py release <playerId>            # dry run, apply without confirmRelease (refused x2), apply
Every write first checks GET /session -> isInvestigationSave true, and stops otherwise.
"""
import json
import sys
import urllib.error
import urllib.request

API = "http://localhost:5120/api/v1"
BRIDGE = "http://localhost:5121"


def call(method, url, body=None):
    data = None if body is None else json.dumps(body).encode()
    request = urllib.request.Request(url, data=data, method=method, headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(request, timeout=180) as response:
            return response.status, json.loads(response.read() or b"null")
    except urllib.error.HTTPError as error:
        return error.code, error.read().decode("utf-8", "replace")


def guard():
    _, session = call("GET", BRIDGE + "/session")
    if not isinstance(session, dict) or session.get("isInvestigationSave") is not True:
        sys.exit(f"STOP: not the test save: {session}")
    _, live = call("GET", API + "/matches/live")
    if isinstance(live, dict) and live.get("inProgress"):
        sys.exit("STOP: a match is being played")


def post(route, body, query=""):
    status, result = call("POST", f"{API}/contracts/{route}{query}", body)
    print(route, query or "(dry run)", status, json.dumps(result, ensure_ascii=False)[:1500])
    return status, result


def contract(player):
    _, contracts = call("GET", API + "/contracts")
    return next((c for c in contracts if c["playerId"] == player), None)


def in_squad(player):
    _, squad = call("GET", API + "/squad")
    players = squad.get("players", squad) if isinstance(squad, dict) else squad
    return any(p.get("id") == player or p.get("playerId") == player for p in players)


def no_dialog():
    request = urllib.request.Request(BRIDGE + "/ui?maxDepth=40")
    with urllib.request.urlopen(request, timeout=60) as response:
        ui = response.read().decode("utf-8", "replace")
    return not any("Panel#GenericModalDialog" in line and "panel--visible" in line for line in ui.splitlines())


def check(ok, what):
    print(("PASS " if ok else "FAIL ") + what)
    if not ok:
        sys.exit(1)


def status_case(player, wanted, original_label):
    original = contract(player)["agreedPlayingTime"]
    for _ in range(2):
        status, plan = post("squad-status", {"playerId": player, "status": wanted})
        check(status == 200 and not plan["applied"], "dry run plans and writes nothing")
        guard()
        status, change = post("squad-status", {"playerId": player, "status": wanted}, "?apply=true")
        check(status == 200 and change["applied"], f"status set to {wanted}: {change.get('agreedPlayingTimeAfter')}")
        guard()
        check(no_dialog(), "no dialog left open")
        guard()
        status, back = post("squad-status", {"playerId": player, "status": original_label}, "?apply=true")
        check(status == 200 and contract(player)["agreedPlayingTime"] == original, f"status back to {original}")
        check(no_dialog(), "no dialog left open")
    status, _ = post("squad-status", {"playerId": player, "status": "Inexistente"})
    check(status == 400, "a status the menu doesn't have is refused")


def dof_case(player):
    status, plan = post("dof-renewal", {"playerId": player})
    check(status == 200 and not plan["applied"], "dry run plans and writes nothing")
    guard()
    status, change = post("dof-renewal", {"playerId": player}, "?apply=true")
    check(status == 200 and change["applied"], f"handed to the DoF, dialog: {change.get('dialogAnswered')}")
    check(no_dialog(), "no dialog left open")
    for query in ("", "?apply=true"):
        status, _ = post("dof-renewal", {"playerId": player}, query)
        check(status == 400, "once handed over, the menu no longer offers it")


def release_case(player):
    status, plan = post("release", {"playerId": player, "how": "Free"})
    check(status == 200 and not plan["applied"], f"dry run shows the cost: {plan.get('confirmation')}")
    for _ in range(2):
        status, _ = post("release", {"playerId": player, "how": "Free"}, "?apply=true")
        check(status == 400, "apply without confirmRelease is refused")
    guard()
    status, change = post("release", {"playerId": player, "how": "Free"}, "?apply=true&confirmRelease=true")
    check(status == 200 and change["applied"] and change.get("dialogAnswered"), f"released, dialog: {change.get('dialogAnswered')}")
    check(no_dialog(), "no dialog left open")
    check(not in_squad(player), "he's gone from get_squad (if not, continue one game day and run: smoke-71.py gone <id>)")


def menus_case(players):
    for player in players:
        for route, body in (("release", {"how": "Free"}), ("release", {"how": "Mutual"}), ("release", {"how": "EndOfContract"}),
                            ("squad-status", {"status": "?"}), ("dof-renewal", {})):
            post(route, {"playerId": player, **body})


if __name__ == "__main__":
    case, args = sys.argv[1], sys.argv[2:]
    if case == "menus":
        menus_case([int(a) for a in args])
    elif case == "status":
        status_case(int(args[0]), args[1], args[2])
    elif case == "gone":
        check(not in_squad(int(args[0])), "he's gone from get_squad")
    elif case == "dof":
        dof_case(int(args[0]))
    elif case == "release":
        release_case(int(args[0]))
