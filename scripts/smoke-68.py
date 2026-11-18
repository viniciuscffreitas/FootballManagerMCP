"""Smoke of ticket #68 (selling and loans out from the player's menu) against the API on the Windows PC.

Run on the Windows PC, with the test save 'MCPFM investigation' loaded and no match being played:
    python smoke-68.py menus <playerId>...         # read only: every exit action's dry run for each player
    python smoke-68.py list <playerId> <choice>    # x2: SetTransferStatus to <choice> (e.g. "Situação de transferência > Na lista
                                                   # de transferências"), get_squad unchanged by the dry run, the exact list
                                                   # read back and confirmed, then off it again
    python smoke-68.py edges <playerId>            # actions the menu doesn't offer: refused, nothing sent (x2)
    python smoke-68.py recall                      # recall a player out on loan (apply), only if his menu offers it
Every write first checks GET /session -> isInvestigationSave true, and stops otherwise.
"""
import json
import sys
import urllib.error
import urllib.request

API = "http://localhost:5120/api/v1"
BRIDGE = "http://localhost:5121"
ACTIONS = ["SetTransferStatus", "RemoveFromTransferList", "RemoveFromLoanList", "OfferToClubs", "Recall", "ExtendLoan"]


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


def exit_(player, action, choice=None, apply=False):
    if apply:
        guard()
    body = {"playerId": player, "action": action, "choice": choice}
    status, result = call("POST", f"{API}/transfers/exit?apply={'true' if apply else 'false'}", body)
    print(action, choice or "", "apply" if apply else "(dry run)", status, json.dumps(result, ensure_ascii=False)[:1500])
    return status, result


def check(ok, what):
    print(("PASS " if ok else "FAIL ") + what)
    if not ok:
        sys.exit(1)


def menus(players):
    for player in players:
        for action in ACTIONS:
            exit_(player, action)


def squad_keys(player):
    _, squad = call("GET", API + "/squad")
    players = squad.get("players", squad) if isinstance(squad, dict) else squad
    found = next((p for p in players if p.get("id") == player), None)
    check(found is not None, f"get_squad lists player {player}")
    return {s.get("key") for s in found.get("statuses") or []}


def wanted_list(choice):
    """The list the choice puts him on, and the get_squad status key that shows it."""
    top = choice.split(">")[0].strip().lower()
    if "empréstimo" in top or "loan" in top:
        return "loanListed", "ListedForLoan", "RemoveFromLoanList"
    return "transferListed", "TransferListed", "RemoveFromTransferList"


def list_and_back(player, choice):
    flag, key, back = wanted_list(choice)
    keys_before = squad_keys(player)
    check(key not in keys_before, f"he starts off that list: {keys_before}")
    status, plan = exit_(player, "SetTransferStatus", choice)
    check(status == 200 and plan["applied"] == "NotWritten", "dry run plans the status and sends nothing")
    check(plan["choice"] == choice, f"dry run picks exactly {choice}: {plan['choice']}")
    check(squad_keys(player) == keys_before, "get_squad is the same after the dry run: nothing was written")
    status, after = exit_(player, "SetTransferStatus", choice, apply=True)
    check(status == 200 and after["applied"] == "Written", f"apply sends the status: {after}")
    check(after["confirmed"] is True, f"the read back confirms it: {after}")
    check(after["status"][flag] is True, f"the menu shows him on that list ({flag}): {after['status']}")
    keys = squad_keys(player)
    check(key in keys, f"get_squad shows {key}: {keys}")
    status, off = exit_(player, back, apply=True)
    check(status == 200 and off["applied"] == "Written" and off["confirmed"] is True, f"off the list again: {off}")
    check(off["status"][flag] is False, f"the menu shows him off that list: {off['status']}")
    keys = squad_keys(player)
    check(key not in keys, f"get_squad no longer shows {key}: {keys}")


def backups():
    _, listed = call("GET", API + "/saves/backups")
    return json.dumps(listed, sort_keys=True)


def edges(player):
    before = backups()
    for _ in range(2):
        status, _ = exit_(player, "RemoveFromLoanList", apply=True)
        check(status == 400, "taking him off a list he isn't on is refused")
        status, _ = exit_(player, "Recall", apply=True)
        check(status == 400, "recalling a player not out on loan is refused")
    status, _ = exit_(999999999, "Recall")
    check(status == 404, "an unknown player is not found")
    guard()
    status, _ = call("POST", f"{API}/transfers/exit?apply=true", {"playerId": player})
    check(status == 400, "a request without an action is refused")
    check(backups() == before, "no save copy was made for a refused change")


def recall():
    _, youth = call("GET", API + "/youth")
    loans = (youth or {}).get("loans") or [] if isinstance(youth, dict) else []
    for loan in loans:
        player = loan.get("playerId") or loan.get("id")
        status, plan = exit_(player, "Recall")
        if status == 200:
            print(f"menu offers a recall for {player}: {plan['choices']}")
            status, after = exit_(player, "Recall", apply=True)
            check(status == 200 and after["applied"] == "Written" and after["confirmed"] is True, f"the recall is confirmed: {after}")
            check(after["status"]["outOnLoan"] is False, "recalled: no longer out on loan")
            return
    print("no player out on loan has a recall in his menu: not run")


if __name__ == "__main__":
    command, args = sys.argv[1], sys.argv[2:]
    if command == "menus":
        menus([int(a) for a in args])
    elif command == "list":
        for _ in range(2):
            list_and_back(int(args[0]), args[1])
    elif command == "edges":
        edges(int(args[0]))
    elif command == "recall":
        recall()
    else:
        sys.exit(__doc__)
