# modern/ — the replacement stack

- `api/` — .NET 11 minimal API. Endpoints grouped per migration unit in `Endpoints/`;
  connection factory in `Data/Db.cs`. During migration, endpoints call the SAME stored
  procedures as the legacy DAL (shared data access contract) — do not switch a unit to
  inline SQL/EF until it is `retired` and the contract is contracted deliberately.
- `api.tests/` — xunit. Every business rule ported from legacy gets tests here that cite
  the legacy source in a comment (see `OrderStatusRulesTests`).
- `web/` — Next.js App Router, TypeScript, server components fetching via `lib/api.ts`.
  Keep `lib/api.ts` types in sync with `api/Domain/Models.cs`.

Conventions:
- Preserve legacy response semantics parity depends on: list ordering, NULL vs empty
  string, en-US currency, case-sensitive status strings.
- New endpoints/pages start with a comment `// Migrates: <legacy files> (unit: <id>)`.
- Validation mirrors the legacy validators unless the spec records a deliberate change.
- `dotnet build` + `dotnet test` must pass and `npm run typecheck` must pass before any
  ledger state advance.
