using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.Web;
using OrderFlow.BuildingBlocks.Web.Versioning;
using OrderFlow.Notifications.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddWebDefaults();
builder.Services.AddNotificationsInfrastructure();

var app = builder.Build();

app.UseWebDefaults();
app.MapDefaultEndpoints();
app.MapVersionedGroup("notifications");

await app.RunAsync();

return 0;
