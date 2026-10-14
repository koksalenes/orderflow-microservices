using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.Notifications.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddNotificationsInfrastructure();
builder.Services.AddNotificationsMessaging();

var app = builder.Build();

app.MapDefaultEndpoints();

await app.Services.MigrateNotificationsDatabaseAsync();
await app.RunAsync();

return 0;
