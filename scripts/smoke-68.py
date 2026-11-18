"""Smoke of ticket #68 (selling and loans out from the player's menu) against the API on the Windows PC.

Run on the Windows PC, with the test save 'MCPFM investigation' loaded and no match being played:
    python smoke-68.py menus <playerId>...         # read only: every exit action's dry run for each player
    python smoke-68.py list <playerId> <choice>    # SetTransferStatus to <choice>, the status read back, then off the list
    python smoke-68.py edges <playerId>            # actions the menu doesn't offer: refused, nothing sent (x2)
    python smoke-68.py recall                      # recall a player out on loan, only if his menu offers it
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


def list_and_back(player, choice):
    status, plan = exit_(player, "SetTransferStatus", choice)
    check(status == 200 and not plan["applied"], "dry run plans the status and sends nothing")
    status, after = exit_(player, "SetTransferStatus", choice, apply=True)
    check(status == 200 and after["applied"], "apply sends the status")
    listed = after["status"]["transferListed"] or after["status"]["loanListed"]
    check(listed, f"the menu shows him listed: {after['status']}")
    back = "RemoveFromTransferList" if after["status"]["transferListed"] else "RemoveFromLoanList"
    status, off = exit_(player, back, apply=True)
    check(status == 200 and not (off["status"]["transferListed"] or off["status"]["loanListed"]), "off the list again")


def edges(player):
    for _ in range(2):
        status, _ = exit_(player, "RemoveFromLoanList", apply=True)
        check(status == 400, "taking him off a list he isn't on is refused")
        status, _ = exit_(player, "Recall", apply=True)
        check(status == 400, "recalling a player not out on loan is refused")
    status, _ = exit_(999999999, "Recall")
    check(status == 404, "an unknown player is not found")


def recall():
    _, youth = call("GET", API + "/youth")
    loans = (youth or {}).get("loans") or [] if isinstance(youth, dict) else []
    for loan in loans:
        player = loan.get("playerId") or loan.get("id")
        status, plan = exit_(player, "Recall")
        if status == 200:
            print(f"menu offers a recall for {player}: {plan['choices']}")
            return
    print("no player out on loan has a recall in his menu: not run")


if __name__ == "__main__":
    command, args = sys.argv[1], sys.argv[2:]
    if command == "menus":
        menus([int(a) for a in args])
    elif command == "list":
        list_and_back(int(args[0]), args[1])
    elif command == "edges":
        edges(int(args[0]))
    elif command == "recall":
        recall()
    else:
        sys.exit(__doc__)
