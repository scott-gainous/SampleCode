using ContosoOrders.Api.Data;
using ContosoOrders.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Db>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddDefaultPolicy(policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

var api = app.MapGroup("/api");
api.MapDashboardEndpoints();
api.MapCustomerEndpoints();
api.MapProductEndpoints();
api.MapOrderEndpoints();

app.Run();
