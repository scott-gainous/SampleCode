---
name: migration-status
description: Summarize migration progress from the ledger and recommend the next unit of work. Use at the start of any migration session or when asked "where are we".
---

# Migration status

1. Run `node tools/ledger.mjs status` and `node tools/ledger.mjs verify`.
2. Cross-check claimed states against reality (states require artifacts):
   - `analyzed`+ → spec exists in `migration/specs/` and golden file in `migration/parity/golden/`
   - `api-complete`+ → endpoints exist in `modern/api/Endpoints/` and `dotnet test modern/api.tests` passes (skip test run if no dotnet SDK; say so)
   - `ui-complete`+ → pages exist in `modern/web/app/`
   - `parity-passed`+ → a green result in `migration/parity/results/` newer than the last code change to that unit (if absent, flag that parity should be re-run)
3. Report: table of units/states, any verify failures or artifact mismatches, and a
   recommendation for the next action per STRATEGY.md phase order (read-path before
   write-path; auth in Phase 4; blocked units and why).
4. Do not change any state from this skill.
