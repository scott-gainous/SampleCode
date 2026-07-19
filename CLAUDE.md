# Contoso Orders migration monorepo

Strangler-fig migration: `legacy/` (ASP.NET 3.5 WebForms) is being replaced by
`modern/api` (.NET 11 minimal API) + `modern/web` (Next.js). Both share the single SQL
Server database defined in `db/`. **The migration playbook is `migration/STRATEGY.md` —
read it before doing migration work.**

## Ground rules (apply to every session)

1. **`legacy/` is read-only evidence.** Never modify legacy code except the explicit
   *retire* step of a unit, and only with human approval recorded via the ledger.
2. **Ledger is truth.** Progress lives in `migration/ledger.json`. Read it with
   `node tools/ledger.mjs status`; change it ONLY with `node tools/ledger.mjs set ...`
   (a hook blocks direct edits). `cutover` and `retired` require `--approved-by` after a
   human says so — never self-approve.
3. **Spec before code.** A unit needs `migration/specs/<unit>.md` (and golden fixtures)
   before you implement it. Quirks get an explicit preserve/fix decision.
4. **Never edit golden fixtures to make parity pass.** A divergence is a modern-side bug
   or a deliberate change that must be spec'd first.
5. **Database is shared.** Additive schema changes only while any unit is un-retired;
   new scripts go in `db/migrations/` (see its README). Sproc behavior changes require
   re-running parity for every unit listing that sproc in the ledger.
6. **One unit in flight per session.** Commit at each ledger state transition.

## Commands

| Task | Command |
|---|---|
| Migration progress | `node tools/ledger.mjs status` |
| Advance a unit | `node tools/ledger.mjs set <unit> <state> --note "..."` |
| Validate ledger (CI runs this) | `node tools/ledger.mjs verify` |
| Parity run | `node tools/parity/run-parity.mjs <unit\|all>` (API must be running) |
| DB up + init | `cd db && docker compose up -d && ./init.sh` |
| API | `cd modern/api && dotnet run` (http://localhost:5080) |
| API tests | `dotnet test modern/api.tests` |
| Web | `cd modern/web && npm run dev` (http://localhost:3000) |
| Web typecheck | `cd modern/web && npm run typecheck` |

## Skills

Migration skills (repo-native — see `migration/STRATEGY.md` §6):

- `/migration-status` — summarize ledger, recommend next work
- `/analyze-legacy <unit>` — produce/refresh the unit's behavior spec + goldens
- `/migrate-page <unit>` — full unit migration loop
- `/parity-check <unit>` — run parity, triage divergences

Engineering skills vendored from [mattpocock/skills](https://github.com/mattpocock/skills)
(provenance + update path: `.claude/skills/VENDORED.md`; run `/setup-matt-pocock-skills`
once per clone before first use): `/tdd`, `/implement`, `/code-review`, `/to-spec`,
`/to-tickets`, `/triage`, `/diagnosing-bugs`, `/domain-modeling`, `/codebase-design`,
`/research`, `/prototype`, and more. Use `/tdd` when porting legacy business rules into
`modern/api/Domain/`, and `/diagnosing-bugs` when triaging parity divergences.

## Agent skills

### Issue tracker

GitHub Issues on `scott-gainous/SampleCode` (gh CLI locally; GitHub MCP tools in remote sessions). See `docs/agents/issue-tracker.md`.

### Triage labels

Default vocabulary (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: root `CONTEXT.md` + `docs/adr/` (created lazily); migration specs are the behavioral authority for migration units. See `docs/agents/domain.md`.

## MCP servers (`.mcp.json`)

- **context7** — up-to-date library docs (Next.js, Dapper, xunit...). Ask for docs with
  "use context7". Optional `CONTEXT7_API_KEY` env var raises rate limits.
- **microsoft-learn** — official Microsoft/.NET docs; use for .NET Framework → .NET 11
  behavior differences during migration work.

## Layout pointers

- Legacy pages: `legacy/ContosoOrders.Web/*.aspx(.cs)`; DAL in `Code/Dal/` (calls sprocs
  in `db/schema/002_procs.sql`, except `UserDal` which uses inline SQL).
- Modern endpoints: `modern/api/Endpoints/*.cs`; each file's header comment names the
  legacy unit it migrates. Domain rules ported from legacy live in `modern/api/Domain/`
  with unit tests in `modern/api.tests`.
- Next.js pages mirror legacy pages 1:1; each page's header comment names its unit.
- Unit ↔ files ↔ sprocs mapping: `migration/inventory.md` + `migration/ledger.json`.
