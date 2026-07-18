using System.Data;
using ContosoOrders.Api.Data;
using ContosoOrders.Api.Domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ContosoOrders.Api.Endpoints;

public static class CustomerEndpoints
{
    // Migrates: legacy Customers.aspx + CustomerEdit.aspx (unit: customers)
    public static RouteGroupBuilder MapCustomerEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/customers", async (Db db, bool includeInactive = false) =>
        {
            using var conn = db.Open();
            var customers = await conn.QueryAsync<CustomerSummary>(
                "dbo.usp_Customer_List",
                new { IncludeInactive = includeInactive },
                commandType: CommandType.StoredProcedure);
            return Results.Ok(customers);
        });

        group.MapGet("/customers/{id:int}", async (Db db, int id) =>
        {
            using var conn = db.Open();
            var customer = await conn.QuerySingleOrDefaultAsync<Customer>(
                "dbo.usp_Customer_Get",
                new { CustomerId = id },
                commandType: CommandType.StoredProcedure);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        });

        group.MapPost("/customers", (Db db, CustomerInput input) => SaveAsync(db, null, input));
        group.MapPut("/customers/{id:int}", (Db db, int id, CustomerInput input) => SaveAsync(db, id, input));

        return group;
    }

    private static async Task<IResult> SaveAsync(Db db, int? id, CustomerInput input)
    {
        // Validation parity with CustomerEdit.aspx: company/contact required, email shape checked.
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(input.CompanyName)) errors["companyName"] = ["Required"];
        if (string.IsNullOrWhiteSpace(input.ContactName)) errors["contactName"] = ["Required"];
        if (string.IsNullOrWhiteSpace(input.Email) || !input.Email.Contains('@')) errors["email"] = ["Invalid email"];
        if (errors.Count > 0) return Results.ValidationProblem(errors);

        try
        {
            using var conn = db.Open();
            var newId = await conn.ExecuteScalarAsync<int>(
                "dbo.usp_Customer_Save",
                new
                {
                    CustomerId = id,
                    CompanyName = input.CompanyName.Trim(),
                    ContactName = input.ContactName.Trim(),
                    Email = input.Email.Trim(),
                    Phone = NullIfEmpty(input.Phone),
                    City = NullIfEmpty(input.City),
                    Country = NullIfEmpty(input.Country),
                    input.IsActive
                },
                commandType: CommandType.StoredProcedure);
            return id is null
                ? Results.Created($"/api/customers/{newId}", new { customerId = newId })
                : Results.Ok(new { customerId = newId });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            // Same duplicate-email rule the legacy page surfaces.
            return Results.Conflict(new { error = "A customer with that email already exists." });
        }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
