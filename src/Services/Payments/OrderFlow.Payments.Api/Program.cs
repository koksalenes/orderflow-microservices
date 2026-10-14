using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.Web;
using OrderFlow.BuildingBlocks.Web.Authentication;
using OrderFlow.BuildingBlocks.Web.Versioning;
using OrderFlow.Payments.Api.Wallets;
using OrderFlow.Payments.Infrastructure;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddWebDefaults();
builder.AddJwtAuthentication();
builder.Services.AddPaymentsInfrastructure();

var app = builder.Build();

app.UseWebDefaults();
app.UseJwtAuthentication();
app.MapDefaultEndpoints();
app.MapVersionedGroup("payments").MapWalletEndpoints();

await app.Services.MigratePaymentsDatabaseAsync();
await app.RunAsync();

return 0;
