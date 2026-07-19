# Unit: dashboard

## Sources (legacy evidence)
- `legacy/ContosoOrders.Web/Default.aspx` / `Default.aspx.cs`
- `legacy/ContosoOrders.Web/Code/Dal/DashboardDal.cs`
- `db/schema/002_procs.sql` → `usp_Dashboard_Summary`

## Behavior
1. Shows four tiles: active customer count, non-discontinued product count, open
   (`Pending`) order count, revenue to date.
2. Revenue = Σ(quantity × item unit price) over all orders whose status ≠ `Cancelled`.
3. All values come from a single call to `usp_Dashboard_Summary`; no app-layer math.
4. Data loads on initial GET only (`!IsPostBack`).

## Quirks
| # | Quirk | Decision | Rationale |
|---|-------|----------|-----------|
| 1 | Revenue formatted `{0:C}` with server culture | preserve | Production servers are en-US; modern uses explicit en-US `Intl.NumberFormat` |

## Deliberate changes
None.

## Data contract
- Read-only: `usp_Dashboard_Summary` (Customers, Products, Orders, OrderItems).

## Out of scope
- Any caching (legacy has none; modern uses `cache: "no-store"`).
