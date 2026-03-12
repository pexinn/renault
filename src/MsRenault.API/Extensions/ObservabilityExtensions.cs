using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Exporter;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace MsRenault.API.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddMsRenaultObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = "MsRenault.API";
        var serviceVersion = "1.0.0";

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName, "MsRenault.Auth", "MsRenault.Ingestion", "MsRenault.Messaging", "MsRenault.Consumer")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("MongoDB.Driver") // Use source name for MongoDB
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                        opts.Protocol = OtlpExportProtocol.Grpc;
                    });

                if (configuration.GetValue<bool>("OpenTelemetry:ConsoleExporter"))
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                        opts.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        return services;
    }
}
