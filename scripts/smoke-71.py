"""Smoke of ticket #71 (contracts from the player's menu) against the API on the Windows PC.

Run on the Windows PC, with the test save 'MCPFM investigation' loaded and no match being played:
    python smoke-71.py menus <playerId>...           # read only: what each contract action's menu offers him
    python smoke-71.py status <playerId> <status> <original>  # raise to <status>, then back to <original> (refused: it only goes up)
    python smoke-71.py gone <playerId>               # he's no longer in get_squad
    python smoke-71.py dof <playerId>                # dry run, apply, then the menu no longer offers it (x2)
    python smoke-71.py release <playerId>            # dry run with a cost, refusals, apply with its confirmation; exit 2 until he's gone
A case passes only when the game's read back shows the exact change asked for; HTTP 200 alone never passes.
Every write first checks GET /session -> isInvestigationSave true, and stops otherwise.
"""
import json
import re
import sys
import unicodedata
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


def plain(text):
    letters = unicodedata.normalize("NFD", (text or "").strip().lower())
    return "".join(c for c in letters if unicodedata.category(c) != "Mn")


# A cost as the game writes it: "R$120 mil", "€ 1,2 mi", "£300K".
COST = re.compile(r"(R\$|€|£|\$)\s?\d[\d.,]*(\s?(mil|mi|M|K|k|bi))?")


def applied(change):
    return isinstance(change, dict) and change.get("applied") == "Yes"


def status_case(player, wanted, original_label):
    """One raise of his agreed playing time, read back, then the way back. The game lets it only go up from the menu:
    the statuses below his own send no event, so the way back is refused before anything is sent (test save, 18/11)."""
    original = contract(player)["agreedPlayingTime"]
    check(plain(original) == plain(original_label), f"his status now is {original_label}: {original}")
    check(plain(wanted) != plain(original), f"{wanted} differs from his status now ({original})")
    status, plan = post("squad-status", {"playerId": player, "status": wanted})
    check(status == 200 and plan["applied"] == "No", "dry run plans and writes nothing")
    check(plain(contract(player)["agreedPlayingTime"]) == plain(original), "the dry run left his status as it was")
    guard()
    status, change = post("squad-status", {"playerId": player, "status": wanted}, "?apply=true")
    check(status == 200 and applied(change) and change.get("confirmed") is True, f"applied and confirmed: {change}")
    now = contract(player)["agreedPlayingTime"]
    check(plain(now) == plain(wanted) and plain(now) != plain(original), f"get_contracts shows exactly {wanted}: {now}")
    check(no_dialog(), "no dialog left open")
    guard()
    status, back = post("squad-status", {"playerId": player, "status": original_label}, "?apply=true")
    if status == 400 and "can't be picked" in str(back):
        check(plain(contract(player)["agreedPlayingTime"]) == plain(wanted), "the way back down is refused and nothing changed")
    else:
        check(status == 200 and applied(back) and back.get("confirmed") is True, f"back applied and confirmed: {back}")
        check(plain(contract(player)["agreedPlayingTime"]) == plain(original), f"status back to {original}")
    check(no_dialog(), "no dialog left open")
    status, _ = post("squad-status", {"playerId": player, "status": "Inexistente"})
    check(status == 400, "a status the menu doesn't have is refused")


def dof_case(player):
    status, plan = post("dof-renewal", {"playerId": player})
    check(status == 200 and plan["applied"] == "No", f"dry run plans and writes nothing: {plan.get('choice')}")
    status, again = post("dof-renewal", {"playerId": player})
    check(status == 200, "the dry run left the menu offering it")
    guard()
    status, change = post("dof-renewal", {"playerId": player}, "?apply=true")
    check(status == 200 and applied(change) and change.get("dialog") in ("Answered", "NotShown"),
          f"handed to the DoF, dialog {change.get('dialog')}: {change.get('dialogText')}")
    check(no_dialog(), "no dialog left open")
    # The hand-over took: his menu no longer offers it, which it did right before.
    for query in ("", "?apply=true"):
        status, refusal = post("dof-renewal", {"playerId": player}, query)
        check(status == 400 and "doesn't offer" in str(refusal), "once handed over, the menu no longer offers it")


def release_case(player):
    status, plan = post("release", {"playerId": player, "how": "Free"})
    cost = COST.search(plan.get("confirmation") or "") if status == 200 else None
    check(status == 200 and plan["applied"] == "No" and cost is not None, f"dry run shows the cost: {plan.get('confirmation')}")
    check(in_squad(player), "the dry run left him in the squad")
    for _ in range(2):
        status, _ = post("release", {"playerId": player, "how": "Free"}, "?apply=true")
        check(status == 400 and in_squad(player), "apply without confirmRelease is refused and he stays")
    agreed = plan["confirmation"]
    status, _ = post("release", {"playerId": player, "expectedConfirmation": agreed}, "?apply=true&confirmRelease=true")
    check(status == 400 and in_squad(player), "apply without how is refused and he stays")
    status, _ = post("release", {"playerId": player, "how": "Free"}, "?apply=true&confirmRelease=true")
    check(status == 400 and in_squad(player), "apply without expectedConfirmation is refused and he stays")
    status, _ = post("release", {"playerId": player, "how": "Free", "expectedConfirmation": agreed + " R$1"},
                     "?apply=true&confirmRelease=true")
    check(status == 400 and in_squad(player), "apply with another confirmation than the menu's is refused and he stays")
    guard()
    status, change = post("release", {"playerId": player, "how": "Free", "expectedConfirmation": agreed},
                          "?apply=true&confirmRelease=true")
    check(status == 200 and applied(change) and change.get("dialog") == "Answered"
          and plain(cost.group(0)) in plain(change.get("dialogText")),
          f"released, dialog with the dry run's cost {cost.group(0)}: {change.get('dialogText')}")
    check(no_dialog(), "no dialog left open")
    if change.get("confirmed") is not True:
        print(f"PENDING he's still listed: continue one game day and run: smoke-71.py gone {player}")
        sys.exit(2)
    check(not in_squad(player), "he's gone from get_squad")


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
