-- Stored procedures. The legacy WebForms DAL calls these; the modern API calls the same
-- procs during migration so both stacks share one behavior surface (see STRATEGY.md,
-- "shared data access contract"). CREATE OR ALTER keeps the script idempotent.

GO
CREATE OR ALTER PROCEDURE dbo.usp_Customer_List
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.CustomerId, c.CompanyName, c.ContactName, c.Email, c.Phone,
           c.City, c.Country, c.IsActive, c.CreatedUtc,
           OrderCount = (SELECT COUNT(*) FROM dbo.Orders o WHERE o.CustomerId = c.CustomerId)
    FROM dbo.Customers c
    WHERE (@IncludeInactive = 1 OR c.IsActive = 1)
    ORDER BY c.CompanyName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Get
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerId, CompanyName, ContactName, Email, Phone, City, Country, IsActive, CreatedUtc
    FROM dbo.Customers
    WHERE CustomerId = @CustomerId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Save
    @CustomerId  INT = NULL,
    @CompanyName NVARCHAR(100),
    @ContactName NVARCHAR(100),
    @Email       NVARCHAR(256),
    @Phone       NVARCHAR(30) = NULL,
    @City        NVARCHAR(60) = NULL,
    @Country     NVARCHAR(60) = NULL,
    @IsActive    BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    IF @CustomerId IS NULL OR @CustomerId = 0
    BEGIN
        INSERT INTO dbo.Customers (CompanyName, ContactName, Email, Phone, City, Country, IsActive)
        VALUES (@CompanyName, @ContactName, @Email, @Phone, @City, @Country, @IsActive);
        SELECT NewCustomerId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Customers
        SET CompanyName = @CompanyName, ContactName = @ContactName, Email = @Email,
            Phone = @Phone, City = @City, Country = @Country, IsActive = @IsActive
        WHERE CustomerId = @CustomerId;
        SELECT NewCustomerId = @CustomerId;
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_List
    @IncludeDiscontinued BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProductId, Sku, Name, UnitPrice, UnitsInStock, Discontinued
    FROM dbo.Products
    WHERE (@IncludeDiscontinued = 1 OR Discontinued = 0)
    ORDER BY Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Order_List
    @CustomerId INT = NULL,
    @Status     NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderId, o.CustomerId, c.CompanyName, o.OrderDate, o.Status,
           o.ShipCity, o.ShipCountry,
           OrderTotal = ISNULL((SELECT SUM(oi.Quantity * oi.UnitPrice)
                                FROM dbo.OrderItems oi WHERE oi.OrderId = o.OrderId), 0)
    FROM dbo.Orders o
    JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
    WHERE (@CustomerId IS NULL OR o.CustomerId = @CustomerId)
      AND (@Status IS NULL OR o.Status = @Status)
    ORDER BY o.OrderDate DESC, o.OrderId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Order_GetWithItems
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Result set 1: order header
    SELECT o.OrderId, o.CustomerId, c.CompanyName, o.OrderDate, o.Status, o.ShipCity, o.ShipCountry
    FROM dbo.Orders o
    JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
    WHERE o.OrderId = @OrderId;

    -- Result set 2: line items
    SELECT oi.OrderItemId, oi.ProductId, p.Sku, p.Name, oi.Quantity, oi.UnitPrice,
           LineTotal = oi.Quantity * oi.UnitPrice
    FROM dbo.OrderItems oi
    JOIN dbo.Products p ON p.ProductId = oi.ProductId
    WHERE oi.OrderId = @OrderId
    ORDER BY oi.OrderItemId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Order_UpdateStatus
    @OrderId INT,
    @Status  NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    -- Legacy behavior quirk (preserved intentionally): status strings are validated in the
    -- application layer, not here. Parity tests cover the app-layer rules.
    UPDATE dbo.Orders SET Status = @Status WHERE OrderId = @OrderId;
    SELECT RowsAffected = @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_Summary
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        CustomerCount   = (SELECT COUNT(*) FROM dbo.Customers WHERE IsActive = 1),
        ProductCount    = (SELECT COUNT(*) FROM dbo.Products WHERE Discontinued = 0),
        OpenOrderCount  = (SELECT COUNT(*) FROM dbo.Orders WHERE Status = 'Pending'),
        RevenueToDate   = ISNULL((SELECT SUM(oi.Quantity * oi.UnitPrice)
                                  FROM dbo.OrderItems oi
                                  JOIN dbo.Orders o ON o.OrderId = oi.OrderId
                                  WHERE o.Status <> 'Cancelled'), 0);
END;
GO
