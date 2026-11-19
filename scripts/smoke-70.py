"""Smoke of ticket #70 (withdraw an offer, mark and unmark a director of football's target) against the API on the
Windows PC.

Run on the Windows PC, with the test save 'MCPFM investigation' loaded and no match being played:
    python smoke-70.py read <playerId>...        # read only: the dry run of every action for each player
    python smoke-70.py dof <playerId>            # x2: mark as DoF target, confirmed by the read back, then unmark
    python smoke-70.py withdraw <playerId>       # x1: withdraw the offer waiting for an answer, confirmed by the read back
    python smoke-70.py edges <playerId>          # x2: missing action -> 400; withdraw with no offer -> 400, nothing sent
Every write first checks GET /session -> isInvestigationSave true and that no match is live, and stops otherwise.
The loan offer with an option to buy (the other half of the ticket's smoke) waits on the panel work still open in #70.
"""
import json
import sys
import urllib.error
import urllib.request

API = "http://localhost:5120/api/v1"
BRIDGE = "http://localhost:5121"
ACTIONS = ["WithdrawOffer", "MarkDofTarget", "UnmarkDofTarget"]


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


def target(player, action, apply=False):
    if apply:
        guard()
    body = {"playerId": player} if action is None else {"playerId": player, "action": action}
    status, result = call("POST", f"{API}/transfers/target?apply={'true' if apply else 'false'}", body)
    print(action, "apply" if apply else "(dry run)", status, json.dumps(result, ensure_ascii=False)[:1500])
    return status, result


def check(ok, what):
    print(("PASS " if ok else "FAIL ") + what)
    if not ok:
        sys.exit(1)


# Where he stands, read from his menu by a dry run that is sure to be answered: the one of an action it offers.
def standing(player):
    for action in ACTIONS:
        status, result = target(player, action)
        if status == 200:
            return result
    return None


def dof(player):
    for round_ in (1, 2):
        status, marked = target(player, "MarkDofTarget", apply=True)
        check(status == 200 and marked["applied"] and marked["confirmed"] is True, f"round {round_}: marked, confirmed")
        check(standing(player)["isDofTarget"] is True, f"round {round_}: read back shows him as a DoF target")
        status, again = target(player, "MarkDofTarget", apply=True)
        check(status == 400, f"round {round_}: marking him again is refused")
        status, unmarked = target(player, "UnmarkDofTarget", apply=True)
        check(status == 200 and unmarked["applied"] and unmarked["confirmed"] is True, f"round {round_}: unmarked, confirmed")
        check(standing(player)["isDofTarget"] is False, f"round {round_}: read back shows him off the targets")


def withdraw(player):
    status, before = target(player, "WithdrawOffer")
    check(status == 200 and before["offerPending"] is True, "an offer waits for an answer")
    status, done = target(player, "WithdrawOffer", apply=True)
    check(status == 200 and done["applied"] and done["confirmed"] is True, "withdrawn, confirmed")
    check(standing(player)["offerPending"] is False, "read back shows no offer waiting")


def edges(player):
    for round_ in (1, 2):
        status, _ = target(player, None, apply=True)
        check(status == 400, f"round {round_}: missing action refused")
        before = standing(player)
        if before and before["offerPending"]:
            sys.exit("STOP: this player has an offer waiting; pick one with none for the edge case")
        status, _ = target(player, "WithdrawOffer", apply=True)
        check(status == 400, f"round {round_}: withdraw with no offer refused")
        after = standing(player)
        check(after == before or (after and before and after["offerPending"] == before["offerPending"]
                                  and after["isDofTarget"] == before["isDofTarget"]), f"round {round_}: menu unchanged")


if __name__ == "__main__":
    command, args = sys.argv[1], [int(arg) for arg in sys.argv[2:]]
    if command == "read":
        for player_id in args:
            for name in ACTIONS:
                target(player_id, name)
    elif command == "dof":
        dof(args[0])
    elif command == "withdraw":
        withdraw(args[0])
    elif command == "edges":
        edges(args[0])
    else:
        sys.exit(__doc__)
