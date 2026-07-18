---
name: migrate-page
description: Execute the full migration loop for one unit (spec → API → UI → parity). Argument: unit id from migration/ledger.json.
---

# Migrate unit: $ARGUMENTS

Follow STRATEGY.md §6. One unit only; commit at every ledger transition.

## 0. Preflight
- `node tools/ledger.mjs status` — confirm the unit's current state and start from there
  (this skill is resumable; never redo completed states).
- If state is `not-started`: run the `analyze-legacy` skill first (invoke its steps).
- Ensure DB is up (`cd db && docker compose up -d && ./init.sh`) when you need to run
  anything live. No Docker available → do all static work, note what's unverified.

## 1. API (`analyzed` → `api-complete`)
- Implement endpoints in `modern/api/Endpoints/<Unit>Endpoints.cs` following existing
  files' style: header comment naming the unit, shared sprocs via Dapper, legacy
  semantics preserved except spec'd deliberate changes.
- Port implicit legacy rules into `modern/api/Domain/` classes with xunit tests in
  `modern/api.tests` citing the legacy source.
- Gate: `dotnet build` + `dotnet test` green (if SDK available), endpoints match every
  spec behavior. Then `node tools/ledger.mjs set <unit> api-complete --note "..."`,
  commit `migrate(<unit>): api`.

## 2. UI (`api-complete` → `ui-complete`)
- Implement `modern/web/app/<unit>/...` pages mirroring the legacy page's states
  (filters, empty states, error messages). Types go in `lib/api.ts`, kept in sync with
  `Domain/Models.cs`.
- Gate: `npm run typecheck` and `npm run build` green. Ledger to `ui-complete`, commit
  `migrate(<unit>): ui`.

## 3. Parity (`ui-complete` → `parity-passed`)
- Run the `parity-check` skill for the unit. Fix modern-side divergences; NEVER edit
  goldens to pass. Ledger advance + commit `migrate(<unit>): parity green`.

## 4. Stop
- `cutover` and `retired` are human-gated. Summarize what's ready and ask the maintainer;
  only on explicit approval run `ledger.mjs set <unit> cutover --approved-by <name>`.
