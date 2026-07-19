using System.Data;
using Microsoft.Data.SqlClient;

namespace ContosoOrders.Api.Data;

/// <summary>
/// Connection factory. During the migration the API calls the SAME stored procedures the
/// legacy WebForms DAL calls (the shared data access contract — see migration/STRATEGY.md),
/// so behavior differences can only come from app-layer code, which parity tests cover.
/// </summary>
public sealed class Db(IConfiguration config)
{
    private readonly string _connectionString =
        config.GetConnectionString("ContosoOrders")
        ?? throw new InvalidOperationException("Missing connection string 'ContosoOrders'.");

    public IDbConnection Open() => new SqlConnection(_connectionString);
}
