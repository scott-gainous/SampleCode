# db/ — shared SQL Server database

One database (`ContosoOrders`) serves BOTH the legacy WebForms app and the modern stack.
Every change here can break either side.

- Schema/proc definitions: `schema/` (idempotent; `CREATE OR ALTER` for procs).
- New changes go in `migrations/` as forward-only numbered scripts — read
  `migrations/README.md` for the expand–migrate–contract rules before writing one.
- **Additive only** while any ledger unit is un-retired. Dropping/renaming requires
  `node tools/ledger.mjs check-db-contract <object>` to pass.
- Changing a stored procedure's behavior invalidates parity for every unit that lists it
  in `migration/ledger.json` — re-run parity for all of them.
- Local instance: `docker compose up -d && ./init.sh` (SQL Server 2022, sa password in
  docker-compose.yml, dev only).
