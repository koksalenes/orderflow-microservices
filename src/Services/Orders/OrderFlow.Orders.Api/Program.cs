using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.Web;
using OrderFlow.BuildingBlocks.Web.Versioning;
using OrderFlow.Orders.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddWebDefaults();
builder.Services.AddOrdersInfrastructure();

var app = builder.Build();

app.UseWebDefaults();
app.MapDefaultEndpoints();
app.MapVersionedGroup("orders");

await app.RunAsync();

return 0;
