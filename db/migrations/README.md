# Database migrations

Forward-only, numbered SQL scripts applied after `schema/` and `seed/` by `db/init.sh`
(and by CI). Rules:

1. **Never edit an applied migration.** Add a new numbered script instead.
2. Name scripts `NNN_short-description.sql` (e.g. `003_add-orders-shippedutc.sql`),
   zero-padded so lexical sort equals apply order.
3. Every script must be **idempotent** (`IF NOT EXISTS`, `CREATE OR ALTER`) because
   `init.sh` re-runs the whole directory.
4. **Both apps share this database.** A migration must keep the legacy WebForms app working
   until the corresponding ledger unit is `retired` — additive changes only (new columns
   NULLable or defaulted; never rename/drop a column the legacy DAL reads). See
   `migration/STRATEGY.md`, "expand–migrate–contract".
5. Contract (drop/rename) migrations are allowed only when `tools/ledger.mjs check-db-contract <object>`
   confirms no non-retired unit references the object.
