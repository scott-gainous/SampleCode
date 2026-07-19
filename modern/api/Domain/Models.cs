namespace ContosoOrders.Api.Domain;

public sealed record Customer(
    int CustomerId, string CompanyName, string ContactName, string Email,
    string? Phone, string? City, string? Country, bool IsActive, DateTime CreatedUtc);

public sealed record CustomerSummary(
    int CustomerId, string CompanyName, string ContactName, string Email,
    string? Phone, string? City, string? Country, bool IsActive, DateTime CreatedUtc,
    int OrderCount);

public sealed record CustomerInput(
    string CompanyName, string ContactName, string Email,
    string? Phone, string? City, string? Country, bool IsActive = true);

public sealed record Product(
    int ProductId, string Sku, string Name, decimal UnitPrice, int UnitsInStock, bool Discontinued);

public sealed record OrderSummary(
    int OrderId, int CustomerId, string CompanyName, DateTime OrderDate, string Status,
    string? ShipCity, string? ShipCountry, decimal OrderTotal);

public sealed record OrderItem(
    int OrderItemId, int ProductId, string Sku, string Name, int Quantity,
    decimal UnitPrice, decimal LineTotal);

public sealed record OrderDetail(
    int OrderId, int CustomerId, string CompanyName, DateTime OrderDate, string Status,
    string? ShipCity, string? ShipCountry, decimal OrderTotal, IReadOnlyList<OrderItem> Items);

public sealed record DashboardSummary(
    int CustomerCount, int ProductCount, int OpenOrderCount, decimal RevenueToDate);
