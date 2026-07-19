---
name: analyze-legacy
description: Deep-dive one legacy unit and produce/refresh its behavior spec and golden parity fixtures. Argument: unit id from migration/ledger.json.
---

# Analyze legacy unit: $ARGUMENTS

Goal: a spec good enough that someone who never reads the legacy code can reimplement
the unit exactly.

1. Look up the unit in `migration/ledger.json` and `migration/inventory.md` to get its
   legacy files and sprocs. If the unit id is unknown, stop and list valid ids.
2. Read EVERY legacy file for the unit — `.aspx` markup (validators, control wiring,
   data formats), `.aspx.cs` code-behind (event handlers, hidden rules, redirects),
   the DAL classes it calls, and the sproc bodies in `db/schema/002_procs.sql`.
   Also grep the rest of `legacy/` for references INTO this unit (querystring links,
   shared code) so you catch coupling.
3. Write `migration/specs/<unit>.md` using the template in `migration/specs/README.md`:
   numbered testable behavior statements, quirks table with preserve/fix decisions,
   deliberate changes, data contract. Cite files for every claim.
4. Derive golden fixtures for `migration/parity/golden/<unit>.json` by computing expected
   values BY HAND from `db/seed/001_seed.sql` + the sproc logic (show your arithmetic in
   the session, e.g. revenue sums). Include: default list, each filter, detail/miss (404),
   each validation and business-rule rejection. Mutating checks must be self-neutralizing
   (target seed rows where the mutation is rejected).
5. Any quirk marked **fix** must appear in "Deliberate changes" AND as a `"deliberate"`
   check in the golden file.
6. Advance the ledger: `node tools/ledger.mjs set <unit> analyzed --note "spec + goldens"`.
   Commit as `analyze(<unit>): behavior spec + golden fixtures`.

Do NOT write any modern code in this skill.
