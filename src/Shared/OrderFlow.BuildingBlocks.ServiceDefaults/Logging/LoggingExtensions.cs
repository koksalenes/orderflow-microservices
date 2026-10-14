using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Logging;

internal static class LoggingExtensions
{
    private const string DevelopmentTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static void AddSerilogLogging(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;

        builder.Logging.ClearProviders();

        builder.Services.AddSerilog(
            (services, logger) =>
            {
                logger
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Service", environment.ApplicationName)
                    .Enrich.WithProperty("Environment", environment.EnvironmentName);

                if (environment.IsDevelopment())
                {
                    logger.WriteTo.Console(
                        outputTemplate: DevelopmentTemplate,
                        formatProvider: CultureInfo.InvariantCulture
                    );
                }
                else
                {
                    logger.WriteTo.Console(new RenderedCompactJsonFormatter());
                }
            },
            writeToProviders: true
        );
    }
}
