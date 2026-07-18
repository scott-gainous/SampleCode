# Unit: products

## Sources (legacy evidence)
- `legacy/ContosoOrders.Web/Products.aspx(.cs)`
- `legacy/ContosoOrders.Web/Code/Dal/ProductDal.cs`
- `db/schema/002_procs.sql` → `usp_Product_List`

## Behavior
1. Read-only list, ordered by Name; excludes discontinued products by default.
2. "Include discontinued" toggle re-queries with `@IncludeDiscontinued = 1`.
3. Unit price rendered `{0:C}`.

## Quirks
| # | Quirk | Decision | Rationale |
|---|-------|----------|-----------|
| — | none recorded | | |

## Deliberate changes
None.

## Data contract
- Read-only: `usp_Product_List` (Products).

## Out of scope
- Product create/edit (legacy has none).
