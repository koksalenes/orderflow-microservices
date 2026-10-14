using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.Web;
using OrderFlow.BuildingBlocks.Web.Correlation;
using OrderFlow.Gateway.Http;
using OrderFlow.Gateway.RateLimiting;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddHttpDefaults();
builder.Services.AddGatewayRateLimiting();
builder.Services.AddRequestBodyLimit();
builder.Services.AddResponseCompression();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseHttpDefaults(CorrelationIdPolicy.Edge);
app.UseResponseCompression();
app.UseRateLimiter();
app.UseRequestBodyLimit();

app.MapDefaultEndpoints();
app.MapReverseProxy();

await app.RunAsync();

return 0;
