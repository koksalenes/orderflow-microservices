using Microsoft.AspNetCore.Builder;
using OrderFlow.BuildingBlocks.Web.Cors;
using OrderFlow.BuildingBlocks.Web.Proxy;
using OrderFlow.BuildingBlocks.Web.Versioning;

namespace OrderFlow.BuildingBlocks.Web;

public static class WebDefaultsExtensions
{
    public static WebApplicationBuilder AddWebDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddHttpDefaults();
        builder.Services.AddTrustedProxy();
        builder.Services.AddCorsPolicy();
        builder.Services.AddApiVersioningDefaults();

        return builder;
    }

    public static WebApplication UseWebDefaults(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseForwardedHeaders();
        app.UseHttpDefaults();
        app.UseCors();

        return app;
    }
}
