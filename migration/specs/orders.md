# Unit: orders

## Sources (legacy evidence)
- `legacy/ContosoOrders.Web/Orders.aspx(.cs)`, `OrderDetail.aspx(.cs)`
- `legacy/ContosoOrders.Web/Code/Dal/OrderDal.cs` (status state machine lives here)
- `db/schema/002_procs.sql` → `usp_Order_List/_GetWithItems/_UpdateStatus`

## Behavior
1. List: all orders newest-first (`OrderDate DESC, OrderId DESC`); optional filters
   `customerId` (querystring) and status (dropdown). Total computed per order in sproc.
2. Detail: header (customer, date, ship-to, status) + line items with denormalized
   order-time unit price; order total = Σ line totals, computed app-side from result set 2.
3. Status update: only from `Pending`; targets `Shipped` or `Cancelled`; `Shipped` and
   `Cancelled` are terminal. Enforced in `OrderDal.UpdateStatus`, **not** in the database.
4. Status update UI only visible while status is `Pending`.
5. Admin gate: only user `admin` / role Admin may update status — checked in the
   code-behind click handler only.

## Quirks
| # | Quirk | Decision | Rationale |
|---|-------|----------|-----------|
| 1 | Invalid transitions silently ignored (no error to user beyond generic label) | fix | Silent no-op hides bugs; API must be explicit |
| 2 | Admin gate is UI-only — direct POST would bypass it | fix | Security hole; enforce server-side |
| 3 | Status = case-sensitive magic strings in DB | preserve | Data contract freeze; `OrderStatusRules` encodes exactly |

## Deliberate changes
- Quirk 1 → API returns **409 Conflict** with explanation on invalid transition
  (golden check `orders.invalid-transition`, `"deliberate": true`).
- Quirk 2 → server-side enforcement lands with the `auth` unit; until then the API
  endpoint is only reachable inside the dev network boundary (documented risk).

## Data contract
- Read: `usp_Order_List`, `usp_Order_GetWithItems`. Write: `usp_Order_UpdateStatus`
  (app-layer guarded).

## Out of scope
- Order creation (legacy app has no order-entry UI; orders arrive via an external
  import process that writes the tables directly — do not break its assumptions).
