-- ContosoOrders core schema. Idempotent: safe to re-run.
-- Both the legacy WebForms app and the modern API read/write these tables.

IF OBJECT_ID('dbo.Customers') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
        CompanyName  NVARCHAR(100) NOT NULL,
        ContactName  NVARCHAR(100) NOT NULL,
        Email        NVARCHAR(256) NOT NULL,
        Phone        NVARCHAR(30)  NULL,
        City         NVARCHAR(60)  NULL,
        Country      NVARCHAR(60)  NULL,
        IsActive     BIT NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1),
        CreatedUtc   DATETIME NOT NULL CONSTRAINT DF_Customers_CreatedUtc DEFAULT (GETUTCDATE())
    );
    CREATE UNIQUE INDEX UX_Customers_Email ON dbo.Customers (Email);
END;

IF OBJECT_ID('dbo.Products') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Sku          NVARCHAR(20)  NOT NULL,
        Name         NVARCHAR(120) NOT NULL,
        UnitPrice    MONEY NOT NULL,
        UnitsInStock INT NOT NULL CONSTRAINT DF_Products_UnitsInStock DEFAULT (0),
        Discontinued BIT NOT NULL CONSTRAINT DF_Products_Discontinued DEFAULT (0)
    );
    CREATE UNIQUE INDEX UX_Products_Sku ON dbo.Products (Sku);
END;

IF OBJECT_ID('dbo.Orders') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        CustomerId  INT NOT NULL CONSTRAINT FK_Orders_Customers REFERENCES dbo.Customers (CustomerId),
        OrderDate   DATETIME NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT (GETUTCDATE()),
        -- Status is a legacy magic-string column: Pending | Shipped | Cancelled.
        -- Preserved as-is during migration; see migration/STRATEGY.md ("data contract freeze").
        Status      NVARCHAR(20) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT ('Pending'),
        ShipCity    NVARCHAR(60) NULL,
        ShipCountry NVARCHAR(60) NULL
    );
    CREATE INDEX IX_Orders_CustomerId ON dbo.Orders (CustomerId);
END;

IF OBJECT_ID('dbo.OrderItems') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderId     INT NOT NULL CONSTRAINT FK_OrderItems_Orders REFERENCES dbo.Orders (OrderId),
        ProductId   INT NOT NULL CONSTRAINT FK_OrderItems_Products REFERENCES dbo.Products (ProductId),
        Quantity    INT NOT NULL,
        -- UnitPrice is denormalized at order time (price may change later on Products).
        UnitPrice   MONEY NOT NULL,
        CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0)
    );
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems (OrderId);
END;

IF OBJECT_ID('dbo.Users') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username     NVARCHAR(50) NOT NULL,
        -- Legacy stores SHA1(password) hex — a known weakness; modernization plan is in
        -- migration/inventory.md (unit: auth). Do not change the format until that unit migrates.
        PasswordHash NVARCHAR(64) NOT NULL,
        DisplayName  NVARCHAR(100) NOT NULL,
        Role         NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT ('User')
    );
    CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users (Username);
END;
