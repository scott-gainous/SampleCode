---
name: parity-check
description: Run the parity harness for a unit (or all) and triage divergences. Argument: unit id or "all".
---

# Parity check: $ARGUMENTS

1. Preconditions: DB seeded fresh (`cd db && docker compose up -d && ./init.sh`) and API
   running (`cd modern/api && dotnet run` in background, wait for /healthz). If either is
   impossible in this environment, say exactly what could not run — never fake a result.
2. Run `node tools/parity/run-parity.mjs <unit|all>`.
3. For each FAIL, classify:
   - **Modern bug** → fix in `modern/`, re-run. (Most common.)
   - **Golden error** (fixture arithmetic wrong vs seed + sproc logic) → recompute by
     hand from `db/seed/001_seed.sql`; fix the golden ONLY with the computation shown in
     your summary.
   - **Undocumented legacy quirk** → update the spec first, then decide preserve/fix.
   - **Deliberate change missing its spec entry** → update `migration/specs/<unit>.md`,
     mark the check `"deliberate"`.
4. Green run for a `ui-complete` unit → `node tools/ledger.mjs set <unit> parity-passed
   --note "parity green <date>"` and commit.
5. Report the per-check table and every classification you made.
