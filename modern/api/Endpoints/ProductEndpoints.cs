using System.Data;
using ContosoOrders.Api.Data;
using ContosoOrders.Api.Domain;
using Dapper;

namespace ContosoOrders.Api.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        // Migrates: legacy Products.aspx (unit: products)
        group.MapGet("/products", async (Db db, bool includeDiscontinued = false) =>
        {
            using var conn = db.Open();
            var products = await conn.QueryAsync<Product>(
                "dbo.usp_Product_List",
                new { IncludeDiscontinued = includeDiscontinued },
                commandType: CommandType.StoredProcedure);
            return Results.Ok(products);
        });
        return group;
    }
}
