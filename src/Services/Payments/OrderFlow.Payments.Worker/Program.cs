using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.Payments.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddPaymentsInfrastructure();

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();

return 0;
