# Parity harness

Compares the modern API's behavior against **golden fixtures** captured from the legacy
app running on the seeded database (`db/seed/001_seed.sql`). See STRATEGY.md §5.

## Run

```bash
# API must be running (dotnet run in modern/api) against a freshly seeded local DB.
node tools/parity/run-parity.mjs all
node tools/parity/run-parity.mjs orders --api http://localhost:5080
```

Results are written to `migration/parity/results/<unit>.json` (gitignored) and summarized
on stdout. Exit code 0 = no unexplained divergences.

## Golden files (`golden/<unit>.json`)

```jsonc
{
  "unit": "orders",
  "capturedFrom": "legacy Orders.aspx + OrderDetail.aspx over db/seed 001",
  "checks": [
    {
      "name": "list-default",
      "request": { "method": "GET", "path": "/api/orders" },
      "expectStatus": 200,
      "expect": [ /* subset-matched against the JSON response */ ]
    },
    {
      "name": "invalid-transition",
      "deliberate": "specs/orders.md — quirk 1: legacy silently ignored; modern returns 409",
      "request": { "method": "POST", "path": "/api/orders/5/status", "body": { "status": "Shipped" } },
      "expectStatus": 409
    }
  ]
}
```

Matching is **subset** semantics: every field in `expect` must equal the response;
extra response fields are ignored (so additive API changes don't break parity).
Arrays match by index and must have equal length (ordering is legacy behavior!).

## Rules

- Never edit a golden file to make a run pass. A divergence is a bug in the modern code
  or a **deliberate change** — the latter needs the spec updated first and a
  `"deliberate"` note pointing at the spec section.
- Mutating checks must be self-neutralizing against seed data (e.g. exercise a rejected
  transition), or the runner must be pointed at a disposable re-seeded DB.
- Before cutover of a unit, re-capture from a live legacy instance
  (Windows/IIS) instead of trusting checked-in fixtures.
