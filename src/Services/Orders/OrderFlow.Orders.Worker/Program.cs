using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.Orders.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOrdersInfrastructure();
builder.Services.AddOrdersCatalog();
builder.Services.AddOrdersMessaging();

var app = builder.Build();

app.MapDefaultEndpoints();

await app.Services.WaitForOrdersDatabaseAsync();
await app.RunAsync();

return 0;
