using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.Inventory.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInventoryInfrastructure();
builder.Services.AddInventoryMessaging();

var app = builder.Build();

app.MapDefaultEndpoints();

await app.Services.WaitForInventoryDatabaseAsync();
await app.RunAsync();

return 0;
