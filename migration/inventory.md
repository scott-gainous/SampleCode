# Legacy inventory & unit mapping

Full surface area of `legacy/ContosoOrders.Web`, grouped into migration **units**
(tracked in `ledger.json`). Regenerate/extend with `/analyze-legacy`.

## Units

### `dashboard`
| | |
|---|---|
| Legacy | `Default.aspx(.cs)` |
| Sprocs | `usp_Dashboard_Summary` |
| Modern API | `GET /api/dashboard` (`Endpoints/DashboardEndpoints.cs`) |
| Modern UI | `app/page.tsx` |
| Quirks | Revenue formatted with server culture (`{0:C}`) — **preserve** as en-US (`formatCurrency`) |

### `customers`
| | |
|---|---|
| Legacy | `Customers.aspx(.cs)`, `CustomerEdit.aspx(.cs)` |
| Sprocs | `usp_Customer_List`, `usp_Customer_Get`, `usp_Customer_Save` |
| Modern API | `GET/POST /api/customers`, `GET/PUT /api/customers/{id}` |
| Modern UI | `app/customers/page.tsx` (list). **Edit form not yet built** — required for `ui-complete` |
| Quirks | Duplicate email surfaces as friendly message (SQL 2601/2627) — **preserve** (API → 409). Email regex validator is looser than RFC — **preserve** shape-check only |

### `products`
| | |
|---|---|
| Legacy | `Products.aspx(.cs)` |
| Sprocs | `usp_Product_List` |
| Modern API | `GET /api/products` |
| Modern UI | `app/products/page.tsx` |
| Quirks | none recorded |

### `orders`
| | |
|---|---|
| Legacy | `Orders.aspx(.cs)`, `OrderDetail.aspx(.cs)` |
| Sprocs | `usp_Order_List`, `usp_Order_GetWithItems`, `usp_Order_UpdateStatus` |
| Modern API | `GET /api/orders`, `GET /api/orders/{id}`, `POST /api/orders/{id}/status` |
| Modern UI | `app/orders/page.tsx`, `app/orders/[id]/page.tsx`. **Status-update UI not yet built** |
| Quirks | (1) Status transitions enforced in DAL, invalid ones **silently ignored** — **deliberate change**: API returns 409. (2) Admin-only status update enforced **only in UI** — **deliberate change**: enforced server-side once auth unit lands. (3) Status strings case-sensitive magic strings — **preserve** (`OrderStatusRules`) |

### `auth`
| | |
|---|---|
| Legacy | `Login.aspx(.cs)`, `Code/Security/AuthHelper.cs`, `Code/Dal/UserDal.cs`, FormsAuth config in `Web.config` |
| Tables | `dbo.Users` (unsalted SHA1 hex — known weak) |
| Modern | **not started.** Plan: cookie/JWT auth in API; lazy hash upgrade (verify SHA1 → rehash with modern KDF on successful login); role claims replace `User.IsInRole` checks |
| Quirks | `UserDal` uses inline SQL (only DAL that does); login accepts any case username (DB collation) — verify & preserve |

## Cross-cutting (not units; handled in Phase 4)
- `Site.Master` navigation/shell → `app/layout.tsx` (done as scaffold)
- `Web.config` appSettings (`Orders.PageSize` — currently unused by any page; confirm dead before dropping)
- `Global.asax` error swallowing → API ProblemDetails + structured logging
- `Styles/site.css` → `app/globals.css`
