using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Infrastructure.Logging;

public static class Logging
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger => (context, loggerConfiguration) =>
    {
        var env = context.HostingEnvironment;
        var configuration = context.Configuration;

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", env.ApplicationName)
            .Enrich.WithProperty("Environment", env.EnvironmentName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();

        if (env.IsDevelopment())
        {
            loggerConfiguration.MinimumLevel.Debug();
        }

        var elasticUri = configuration.GetValue<string>("ElasticConfiguration:Uri");

        if (!string.IsNullOrWhiteSpace(elasticUri))
        {
            var dataStream = new DataStreamName(
                "logs",
                env.ApplicationName?.ToLower().Replace('.', '-') ?? "unknown",
                env.EnvironmentName?.ToLower() ?? "development"
            );

            try
            {
                loggerConfiguration.WriteTo.Elasticsearch(
                    new[] { new Uri(elasticUri) },
                    opts =>
                    {
                        opts.DataStream = dataStream;
                        opts.BootstrapMethod = BootstrapMethod.None;
                    },
                    _ => { }
                );
            }
            catch
            {
                // Elasticsearch sink configuration failed (e.g. cluster not reachable yet).
                // Console sink is still configured, so logging will continue to work.
            }
        }
    };
}
