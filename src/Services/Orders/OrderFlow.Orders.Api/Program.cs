using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.Web;
using OrderFlow.BuildingBlocks.Web.Authentication;
using OrderFlow.BuildingBlocks.Web.Versioning;
using OrderFlow.Orders.Api.Orders;
using OrderFlow.Orders.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddWebDefaults();
builder.AddJwtAuthentication();
builder.Services.AddOrdersInfrastructure();
builder.Services.AddOrdersCatalog();

var app = builder.Build();

app.UseWebDefaults();
app.UseJwtAuthentication();
app.MapDefaultEndpoints();
app.MapVersionedGroup("orders").MapOrderEndpoints();

await app.Services.MigrateOrdersDatabaseAsync();
await app.RunAsync();

return 0;
