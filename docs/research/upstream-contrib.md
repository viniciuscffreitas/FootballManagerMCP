# Upstream MCPFM: contributions and roadmap

Research for fork issue #7. Checked on 2026-11-16 against `upstream` = FootballManagerMCP/FootballManagerMCP at
`bbbd17c` (fork `main` is the same commit).

## Answer

- Upstream accepts issues and pull requests. Every change to `main` needs a PR, a green `build-and-test` check and one
  approving review. Plugin changes must say what was tried in game and on which game version.
- Nothing is visibly in progress upstream. The only branch is `main`, there are no open PRs or issues, and the two
  merged PRs deleted their branches. The author works on local branches and publishes finished batches.
- The README roadmap names our priorities as "next up": replying to messages and the match-day flow (continue, team
  talks, in-match changes). Training, the first item on that list, already shipped in PR #2. So upstream will likely
  build messages and match day soon, from code we cannot see.
- Recommendation: work fork-first, keep each feature in upstream's style so it can become a PR, and open upstream PRs
  only for small, self-contained pieces. Do not plan around upstream delivering a feature on a date.

## Contribution rules

| Rule | Source |
|---|---|
| "Issues and pull requests are welcome. Every change to `main` goes through a pull request: CI (the build and the tests) has to pass, and a maintainer reviews it." | README.md, Contributing |
| Run `dotnet test tests/FootballManager.Tests`; add tests for logic that runs without the game | README.md, Contributing |
| Plugin or game-path changes: state in the PR what was tried, on which game version | README.md, Contributing |
| Style: Mapster for every mapping, one type per file, braces on every `if`, comments that say what the game does and why | README.md, Contributing |
| Ruleset "Protect main" (active, default branch): no deletion, no force push, PR required with 1 approval, stale reviews dismissed, review threads resolved, status check `build-and-test` required. No bypass actors listed | `gh api repos/FootballManagerMCP/FootballManagerMCP/rulesets/27008972` |
| CI builds and tests everything except the plugin (it needs FM27 + BepInEx), plus the API Docker image, on push to main and every PR. Token is `contents: read` | `.github/workflows/ci.yml` |
| No CONTRIBUTING file, no issue/PR templates, no CODEOWNERS. `.github/` holds only `workflows/ci.yml` | repo tree at `bbbd17c` |
| Licence MIT, copyright 2026 Leo Paradowski. Forking and keeping changes fork-only is allowed | LICENSE |

## Project activity

- Org `FootballManagerMCP` created 2026-11-11, one public repo. Repo created 2026-11-11, last push 2026-11-14.
  2 stars, 1 fork (ours), discussions off. (`gh api repos/...`, `gh api orgs/FootballManagerMCP`)
- One contributor: `lparadowski` (Leo Paradowski), 24 commits from 2026-10-31 to 2026-11-14. (`gh api .../contributors`,
  `git log upstream/main`)
- PRs, all by the author, both merged, no reviews or comments on them:
  - #1 "Contributing note and read-only CI token" (branch `contributing`), 2026-11-11. Body: "First pull request under
    the new rules on `main`."
  - #2 "Training, competition stats and named shortlists" (branch `training`), merged 2026-11-14. Body lists in-game
    checks on FM 27.0.1.2503222 and "99 unit tests pass".
- Issues: none, open or closed. Branches: only `main`. Releases and tags: none.

So the review rule is new and untested with outside contributors. The author self-merges with an approval requirement
in place; an external PR depends on him reviewing.

## Roadmap and what overlaps us

README, Roadmap: "The goal is an AI that can manage a club on its own while you watch. Next up: training, replying to
messages, and the match-day flow (continuing the game, team talks, in-match changes)."

README, Known limits: "Not yet covered: training positions and roles, replying to messages, making transfer or
contract offers, continuing the game and match days."

| Our priority | Upstream state at `bbbd17c` | Overlap risk |
|---|---|---|
| Reply to messages | Roadmap "next up". Groundwork exists: `get_inbox` already reads `CanRespond` and `NewsHasResponded` (`src/FootballManager/FootballManager.Infrastructure/Constants/InboxConstants.cs`). No reply write | High |
| Continue the game | Roadmap "next up". Plugin already has `/continue` (`src/Plugin/FootballManager.Plugin/Session/ContinueButton.cs`): raises `ContinueGameEvent`, needs `confirm=true`, behind the write guard. Not exposed as an MCP tool on purpose (README, Safety) | High |
| Match day: team talks, in-match changes | Roadmap "next up". Nothing in code. The write guard refuses every change during a match (README, Safety) | High |
| Create a tactic in an empty slot | Not on the roadmap. `set_tactic` refuses: "Slot {n} holds no tactic; create one there in the game first." (`TacticEditingService.cs:111`) | Low |
| Lineup | Already covered by `set_tactic` (lineup) and plugin `/pick`, `/move` | Low, extend existing code |
| Transfers / contracts | In "Not yet covered", but not in "Next up". Reads only (contract info in `get_competition_stats`, `TransferListed` status in `get_squad`) | Medium-low |

No unpublished work is visible: `git fetch upstream` brings only `main`, and the branches API lists only `main`.
In-progress work, if any, lives on the author's machine.

## Stated constraints

- Windows 10/11, FM27 on Steam only. Built on 27.0.0.2493426; reads also checked on 27.0.1.2494769 (README,
  Requirements). PR #2 was checked on 27.0.1.2503222.
- Game tested in French. Tools use the game's ids, so other languages "should work, but they haven't been tested"
  (README, Known limits).
- BepInEx 6.0.0-be.791 IL2CPP, .NET 10 SDK. The plugin targets .NET 6 (README).
- Safety posture the author cares about: every write is a dry run unless `apply: true`; no saves, no continue, no
  load/quit exposed as tools; no writes during a match. A PR that exposes continue or match-day writes cuts against
  this and needs a strong safety story (README, Safety).

## Recommendation

1. Fork-first. Our goal (Claude plays alone and headless) goes further than upstream's safety stance, which keeps
   continue and match-day writes out of the tools. Those pieces belong in the fork until the author opens that door.
2. Write fork code in upstream style (Mapster, one type per file, tests without the game, in-game check notes). That
   keeps the PR path open and makes rebasing on upstream cheap.
3. Good PR candidates, small and outside the "next up" list: create a tactic in an empty slot; lineup fixes.
   Messages reply is a candidate only if it lands before the author's own version; otherwise expect conflicts.
4. Before starting messages, continue or match day: `git fetch upstream` and check for new branches or PRs. Rebase the
   fork on upstream `main` after each upstream merge.
5. Do not open issues or PRs upstream from this research. Any outreach is the Vini's call.
