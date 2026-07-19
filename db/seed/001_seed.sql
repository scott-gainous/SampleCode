-- Development seed data. Guarded: only runs into an empty database.
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    PRINT 'Seed skipped: data already present.';
    RETURN;
END;

INSERT INTO dbo.Customers (CompanyName, ContactName, Email, Phone, City, Country) VALUES
 (N'Fabrikam Inc',        N'Ava Chen',        N'ava@fabrikam.example',    N'555-0101', N'Seattle',   N'USA'),
 (N'Litware LLC',         N'Sam Ortiz',       N'sam@litware.example',     N'555-0102', N'Denver',    N'USA'),
 (N'Tailspin Toys',       N'Riley Novak',     N'riley@tailspin.example',  N'555-0103', N'Toronto',   N'Canada'),
 (N'Wide World Importers',N'Priya Raman',     N'priya@wwi.example',       N'555-0104', N'London',    N'UK'),
 (N'Proseware GmbH',      N'Jonas Keller',    N'jonas@proseware.example', N'555-0105', N'Berlin',    N'Germany');

UPDATE dbo.Customers SET IsActive = 0 WHERE CompanyName = N'Proseware GmbH';

INSERT INTO dbo.Products (Sku, Name, UnitPrice, UnitsInStock, Discontinued) VALUES
 (N'WID-100', N'Standard Widget',        12.50, 340, 0),
 (N'WID-200', N'Premium Widget',         29.99, 120, 0),
 (N'GAD-050', N'Pocket Gadget',           8.75, 560, 0),
 (N'GAD-090', N'Deluxe Gadget',          47.00,  35, 0),
 (N'LEG-001', N'Legacy Gizmo (retired)', 19.95,   0, 1);

INSERT INTO dbo.Orders (CustomerId, OrderDate, Status, ShipCity, ShipCountry) VALUES
 (1, '2026-06-01T14:30:00', N'Shipped',   N'Seattle', N'USA'),
 (1, '2026-07-02T09:15:00', N'Pending',   N'Seattle', N'USA'),
 (2, '2026-06-18T11:00:00', N'Shipped',   N'Denver',  N'USA'),
 (3, '2026-07-05T16:45:00', N'Pending',   N'Toronto', N'Canada'),
 (4, '2026-05-20T08:05:00', N'Cancelled', N'London',  N'UK');

INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES
 (1, 1, 10, 12.50),
 (1, 3,  5,  8.75),
 (2, 2,  3, 29.99),
 (3, 4,  2, 47.00),
 (3, 1, 20, 12.50),
 (4, 3, 12,  8.75),
 (5, 2,  1, 29.99);

-- Password format matches the legacy app: SHA1 hex of the plaintext.
-- 'admin' / 'password1'  |  'clerk' / 'password1'
INSERT INTO dbo.Users (Username, PasswordHash, DisplayName, Role) VALUES
 (N'admin', N'e38ad214943daad1d64c102faec29de4afe9da3d', N'Site Administrator', N'Admin'),
 (N'clerk', N'e38ad214943daad1d64c102faec29de4afe9da3d', N'Order Clerk',        N'User');

PRINT 'Seed complete.';
