# Contoso Orders — Legacy-to-Modern Migration Monorepo

This repository is a **strangler-fig migration monorepo**: a legacy ASP.NET 3.5 WebForms
application and its modern replacement (.NET 11 API + Next.js) live side by side, sharing a
single SQL Server database, while an AI-driven (Claude Code) harness migrates functionality
page by page until the legacy app can be retired.

## Repository layout

```
.
├── db/                      # Shared SQL Server database (single source of truth)
│   ├── schema/              #   Tables, constraints, stored procedures
│   ├── seed/                #   Development seed data
│   ├── migrations/          #   Forward-only migration scripts
│   └── docker-compose.yml   #   Local SQL Server 2022 container
├── legacy/
│   └── ContosoOrders.Web/   # ASP.NET 3.5 WebForms app (the system being migrated)
├── modern/
│   ├── api/                 # .NET 11 minimal API (ContosoOrders.Api)
│   └── web/                 # Next.js frontend (App Router, TypeScript)
├── migration/
│   ├── STRATEGY.md          # The AI migration strategy — read this first
│   ├── inventory.md         # Legacy surface-area inventory + legacy→modern mapping
│   ├── ledger.json          # Machine-readable per-unit migration status
│   └── parity/              # Parity-testing harness (legacy vs. modern comparison)
├── tools/                   # Scripts used by CI and the agentic harness
├── .claude/                 # Claude Code harness: skills, subagents, permissions
└── CLAUDE.md                # Root guidance for Claude Code sessions
```

## Quick start

### 1. Database (required by both apps)

```bash
cd db
docker compose up -d          # SQL Server 2022 on localhost:1433 (sa / see docker-compose.yml)
./init.sh                     # applies schema/, then seed/, then migrations/
```

### 2. Modern stack

```bash
# API — http://localhost:5080
cd modern/api && dotnet run

# Web — http://localhost:3000
cd modern/web && npm install && npm run dev
```

### 3. Legacy app

The WebForms app targets .NET Framework 3.5 and requires Windows + IIS/IIS Express:

```powershell
# From a Windows dev box or VM, with .NET Framework 3.5 enabled:
msbuild legacy/ContosoOrders.Web/ContosoOrders.Web.csproj
# then map an IIS site (or IIS Express) to legacy/ContosoOrders.Web
```

On non-Windows machines, treat `legacy/` as **read-only source of truth** for behavior —
that is exactly how the migration harness uses it.

## The migration

The strategy, phases, rules, and definition of done live in
[`migration/STRATEGY.md`](migration/STRATEGY.md). Progress is tracked in
[`migration/ledger.json`](migration/ledger.json) — never edit status by hand; use
`node tools/ledger.mjs`.

To run a migration session with Claude Code, start in the repo root and use the skills:

| Skill | What it does |
|---|---|
| `/migration-status` | Summarize the ledger: what's done, in flight, and next up |
| `/analyze-legacy <unit>` | Deep-dive a legacy page/module and write a behavior spec |
| `/migrate-page <unit>` | Execute a full migration of one unit (spec → API → UI → parity) |
| `/parity-check <unit>` | Run the parity harness for a unit and report divergences |

## CI

`azure-pipelines.yml` builds the modern API and web app, lints the SQL, and validates the
migration ledger on every push. The legacy app is built on a Windows pool (compile-only,
as a regression guard) — see the pipeline file for details.
