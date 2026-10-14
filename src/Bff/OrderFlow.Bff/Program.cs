using OrderFlow.Bff.Authentication;
using OrderFlow.Bff.Endpoints;
using OrderFlow.Bff.Proxy;
using OrderFlow.Bff.Security;
using OrderFlow.Bff.Session;
using OrderFlow.BuildingBlocks.ServiceDefaults;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.Web;
using OrderFlow.BuildingBlocks.Web.Correlation;

if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddHttpDefaults();
builder.Services.AddBffSession();
builder.Services.AddBffAuthentication();
builder.Services.AddBffProxy(builder.Configuration);

var app = builder.Build();

app.UseHttpDefaults(CorrelationIdPolicy.Edge);
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<CsrfHeaderMiddleware>();

app.MapDefaultEndpoints();
app.MapBffEndpoints();
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

await app.RunAsync();

return 0;
