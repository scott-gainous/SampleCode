# AI Migration Strategy: WebForms → .NET 11 + Next.js

**Goal:** retire `legacy/ContosoOrders.Web` (ASP.NET 3.5 WebForms) by rebuilding its
functionality in `modern/api` (.NET 11 minimal API) and `modern/web` (Next.js), with the
migration executed primarily by Claude Code sessions running inside this repository.

This document is the contract every migration session follows. The repo layout, the
ledger, the parity harness, and the `.claude/` skills all exist to make that execution
mechanical, verifiable, and resumable.

---

## 1. Core principles

1. **Strangler fig, not big bang.** The legacy app keeps running throughout. Functionality
   moves one *unit* at a time; a unit is only removed from the legacy app (retired) after
   its modern replacement has passed parity and taken production traffic.
2. **The database is the seam.** Both stacks share the single `ContosoOrders` SQL Server
   database in `db/`. During migration the modern API calls the **same stored procedures**
   as the legacy DAL (the *shared data access contract*). This collapses the parity
   problem: any divergence must come from app-layer code, which the parity suite covers.
3. **Behavior first, code second.** Legacy code is *evidence of behavior*, not a template.
   Each unit gets a written behavior spec (`migration/specs/<unit>.md`) extracted from the
   legacy source — including the quirks — before any modern code is written.
4. **Quirks are decisions, not accidents.** When legacy behavior is surprising (silent
   failure, UI-only authorization, culture-dependent formatting), the spec records it and
   explicitly decides: *preserve* (parity-tested) or *intentionally fix* (documented in the
   spec's "Deliberate changes" section, excluded from parity comparison).
5. **The ledger is the single source of truth for progress.** `migration/ledger.json`,
   updated only via `node tools/ledger.mjs`. Any session can be interrupted and any new
   session can resume from the ledger alone.
6. **Legacy is read-only** except for the final *retire* step of a unit. Bug fixes needed
   mid-migration land in the modern stack; only critical production fixes touch legacy,
   and they must be mirrored into the affected unit's spec.

## 2. Unit model and lifecycle

A **unit** is the smallest independently shippable slice of legacy functionality —
typically one page or a tightly coupled page pair. Units and their legacy→modern mapping
are enumerated in [`inventory.md`](inventory.md).

Each unit moves through this state machine (enforced by `tools/ledger.mjs`):

```
not-started → analyzed → api-complete → ui-complete → parity-passed → cutover → retired
```

| State | Meaning | Exit criteria |
|---|---|---|
| `not-started` | No work done | — |
| `analyzed` | Behavior spec written | `migration/specs/<unit>.md` exists; quirks classified preserve/fix; golden fixtures captured |
| `api-complete` | API endpoints implemented | Endpoints exist, unit tests pass, calls shared sprocs |
| `ui-complete` | Next.js pages implemented | UI renders all spec'd states; typecheck/build pass |
| `parity-passed` | Harness green | `node tools/parity/run-parity.mjs <unit>` reports zero unexplained divergences |
| `cutover` | Modern serves real traffic | Reverse proxy / links route users to modern; legacy page still available as fallback |
| `retired` | Legacy page removed | Legacy page deleted (or stubbed to redirect); DB contract check run before any schema contraction |

## 3. Phases

**Phase 0 — Foundation (done in this repo).** Monorepo layout, shared DB under `db/`,
both app skeletons, ledger, parity harness, Claude Code harness.

**Phase 1 — Inventory & analysis.** Enumerate every legacy page, control, sproc, config
setting, and implicit business rule into `inventory.md`; write specs for the first units.
The `/analyze-legacy` skill drives this.

**Phase 2 — Read-path migration.** Migrate read-only units first (dashboard, lists,
detail views). Lowest risk, exercises the whole pipeline end to end.

**Phase 3 — Write-path migration.** Mutating units (customer edit, order status). These
carry the business rules; every rule gets a unit test in `modern/api.tests` *and* a parity
check. UI-layer-only rules in legacy (e.g. admin-only status updates) are promoted to
server-side enforcement — a *deliberate change*, recorded in the spec.

**Phase 4 — Cross-cutting.** Auth (FormsAuth/SHA1 → modern identity with hash upgrade on
login), error handling, logging, configuration.

**Phase 5 — Cutover & contraction.** Route all traffic to modern, retire legacy pages
unit by unit, then contract the database (drop legacy-only sprocs, tighten schema) using
`tools/ledger.mjs check-db-contract` as the gate. Delete `legacy/` last.

## 4. Database rules (expand–migrate–contract)

- While any unit is not `retired`: **additive schema changes only.** New columns NULLable
  or defaulted. Never rename/drop anything the legacy DAL touches.
- Stored procedures are the shared contract: behavior changes to a sproc require
  re-running parity for every unit that calls it (the ledger maps units → sprocs).
- Contractions (drops/renames) require `check-db-contract` to confirm no non-retired unit
  references the object.
- All changes are forward-only numbered scripts in `db/migrations/` (see its README).

## 5. Parity testing

The parity harness (`migration/parity/`, runner `tools/parity/run-parity.mjs`) compares
modern behavior against **golden fixtures** captured from the legacy app running on the
seeded database. Golden files live in `migration/parity/golden/<unit>.json`.

- Fixtures are captured once per unit during analysis (Phase 1) and updated only when a
  *deliberate change* is approved.
- The runner hits the modern API and deep-compares responses. Divergences are reported
  per-field; a unit cannot pass `parity-passed` with unexplained divergences.
- Deliberate changes are encoded in the golden file (`"deliberate": true` on a check)
  so the harness asserts the *new* intended behavior and links to the spec section.
- When a live legacy instance is available (Windows/IIS), `--live <legacy-base-url>`
  re-captures fixtures instead of trusting checked-in ones — use before cutover.

## 6. How the AI executes a unit (session protocol)

Every Claude Code migration session follows this loop, encoded in `.claude/skills/`:

1. `/migration-status` — read the ledger, pick the next unit (or take one as an argument).
2. `/analyze-legacy <unit>` — read every legacy file for the unit, write/refresh the spec,
   list quirks with preserve/fix decisions, capture or verify golden fixtures.
3. `/migrate-page <unit>` — implement API endpoints (with unit tests), then the Next.js
   UI, updating the ledger state as each exit criterion is met. Small commits per state
   transition.
4. `/parity-check <unit>` — run the harness; fix divergences or (with human approval)
   record a deliberate change; advance to `parity-passed`.
5. Human review gates `cutover` and `retired` — the AI proposes, a maintainer approves.
   These two transitions are never taken autonomously.

**Session rules** (also in the root `CLAUDE.md`):
- One unit in flight per session. Finish or record precisely where you stopped
  (`ledger.mjs set <unit> <state> --note`).
- Never edit `ledger.json` by hand; never edit golden fixtures to make a test pass —
  divergences are either bugs to fix or deliberate changes to document.
- Commit at every ledger transition so history maps 1:1 to migration progress.

## 7. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Hidden behavior in legacy (ViewState, event order, culture formatting) | Spec-first analysis; quirk classification; parity fixtures capture real output |
| Both stacks writing the same tables during transition | Shared sproc contract; write-path units migrated one at a time; sproc changes trigger cross-unit parity reruns |
| AI drift / hallucinated progress | Ledger is machine-validated in CI (`ledger.mjs verify`); states require artifacts (spec file, passing tests) that CI checks exist |
| Auth mismatch during coexistence | Phase 4 dedicates a unit to auth; until then modern stack is deployed behind the same network boundary; SHA1 hashes upgraded lazily at first modern login |
| Schema drift breaking legacy | Expand-only rule + `check-db-contract` gate + legacy compile job in CI |

## 8. Definition of done (whole migration)

- Every unit in `ledger.json` is `retired`.
- `legacy/` deleted; `db/` contracted to modern-only surface.
- Parity harness archived (goldens kept as regression fixtures for the modern stack).
- CI no longer carries the Windows/MSBuild job.
