# Unit: customers

## Sources (legacy evidence)
- `legacy/ContosoOrders.Web/Customers.aspx(.cs)`, `CustomerEdit.aspx(.cs)`
- `legacy/ContosoOrders.Web/Code/Dal/CustomerDal.cs`
- `db/schema/002_procs.sql` → `usp_Customer_List/_Get/_Save`

## Behavior
1. List shows active customers by default, ordered by CompanyName; "include inactive"
   toggle re-queries with `@IncludeInactive = 1`.
2. List includes per-customer order count (computed in the sproc).
3. Edit form: CompanyName + ContactName required; email validated by shape only
   (legacy regex `\w+([-+.']\w+)*@...` — not RFC).
4. Save is an upsert via `usp_Customer_Save` (id 0/NULL → insert, else update);
   returns the customer id.
5. Empty optional fields (Phone/City/Country) are stored as NULL, not empty string.
6. Duplicate email (unique index, SQL errors 2601/2627) → friendly "already exists"
   message; all other SQL errors bubble up.

## Quirks
| # | Quirk | Decision | Rationale |
|---|-------|----------|-----------|
| 1 | Email validation is shape-only, looser than RFC | preserve | Same data admissible in both stacks during coexistence |
| 2 | No optimistic concurrency — last write wins | preserve | Fixing would change legacy behavior mid-migration; revisit post-cutover |

## Deliberate changes
- Duplicate email → HTTP 409 with `{ "error": ... }` (legacy shows inline label).
  Same rule, HTTP-appropriate surface. Golden check: `customers.save-duplicate`.

## Data contract
- Read: `usp_Customer_List`, `usp_Customer_Get`. Write: `usp_Customer_Save` (upsert).

## Out of scope
- Customer delete (legacy has none — deactivate via IsActive only).
