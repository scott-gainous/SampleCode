using System.Data;
using ContosoOrders.Api.Data;
using ContosoOrders.Api.Domain;
using Dapper;

namespace ContosoOrders.Api.Endpoints;

public static class OrderEndpoints
{
    public sealed record StatusUpdate(string Status);

    // Migrates: legacy Orders.aspx + OrderDetail.aspx (unit: orders)
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/orders", async (Db db, int? customerId, string? status) =>
        {
            if (status is not null && !OrderStatusRules.IsValidStatus(status))
            {
                return Results.BadRequest(new { error = $"Unknown status '{status}'." });
            }
            using var conn = db.Open();
            var orders = await conn.QueryAsync<OrderSummary>(
                "dbo.usp_Order_List",
                new { CustomerId = customerId, Status = status },
                commandType: CommandType.StoredProcedure);
            return Results.Ok(orders);
        });

        group.MapGet("/orders/{id:int}", async (Db db, int id) =>
        {
            using var conn = db.Open();
            using var multi = await conn.QueryMultipleAsync(
                "dbo.usp_Order_GetWithItems",
                new { OrderId = id },
                commandType: CommandType.StoredProcedure);

            var header = await multi.ReadSingleOrDefaultAsync<OrderHeaderRow>();
            if (header is null) return Results.NotFound();

            var items = (await multi.ReadAsync<OrderItem>()).ToList();
            var detail = new OrderDetail(
                header.OrderId, header.CustomerId, header.CompanyName, header.OrderDate,
                header.Status, header.ShipCity, header.ShipCountry,
                items.Sum(i => i.LineTotal), items);
            return Results.Ok(detail);
        });

        group.MapPost("/orders/{id:int}/status", async (Db db, int id, StatusUpdate update) =>
        {
            if (!OrderStatusRules.IsValidStatus(update.Status))
            {
                return Results.BadRequest(new { error = $"Unknown status '{update.Status}'." });
            }

            using var conn = db.Open();
            var current = await conn.ExecuteScalarAsync<string?>(
                "SELECT Status FROM dbo.Orders WHERE OrderId = @OrderId", new { OrderId = id });
            if (current is null) return Results.NotFound();

            // Same transition graph as legacy OrderDal.UpdateStatus, but the legacy app
            // silently ignored invalid transitions — the API reports them as 409.
            if (!OrderStatusRules.CanTransition(current, update.Status))
            {
                return Results.Conflict(new { error = $"Cannot change status from '{current}' to '{update.Status}'." });
            }

            await conn.ExecuteAsync(
                "dbo.usp_Order_UpdateStatus",
                new { OrderId = id, update.Status },
                commandType: CommandType.StoredProcedure);
            return Results.Ok(new { orderId = id, status = update.Status });
        });

        return group;
    }

    private sealed record OrderHeaderRow(
        int OrderId, int CustomerId, string CompanyName, DateTime OrderDate,
        string Status, string? ShipCity, string? ShipCountry);
}
