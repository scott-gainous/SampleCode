using System.Data;
using ContosoOrders.Api.Data;
using ContosoOrders.Api.Domain;
using Dapper;

namespace ContosoOrders.Api.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder group)
    {
        // Migrates: legacy Default.aspx (unit: dashboard)
        group.MapGet("/dashboard", async (Db db) =>
        {
            using var conn = db.Open();
            var summary = await conn.QuerySingleAsync<DashboardSummary>(
                "dbo.usp_Dashboard_Summary", commandType: CommandType.StoredProcedure);
            return Results.Ok(summary);
        });
        return group;
    }
}
