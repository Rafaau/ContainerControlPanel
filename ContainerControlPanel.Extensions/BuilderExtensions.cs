using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace ContainerControlPanel.Extensions;

/// <summary>
/// Extension methods for WebApplicationBuilder.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    /// Adds OpenTelemetry to the application.
    /// </summary>
    /// <param name="builder"><see cref="WebApplicationBuilder"/> instance.</param>
    /// <param name="host">Host name.</param>
    /// <param name="port">Port number.</param>
    /// <param name="resourceName">Name of the resource.</param>
    public static void AddOpenTelemetry(this WebApplicationBuilder builder, string host, int port, string resourceName)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(resourceName))
                    // Removed for request and response body logging
                    //.AddAspNetCoreInstrumentation(options =>
                    //{
                    //    options.EnrichWithHttpRequest = (activity, httpContext) =>
                    //    {
                    //        if (Activity.Current?.ParentId == null)
                    //        {
                    //            activity.SetParentId(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom());
                    //        }
                    //    };
                    //})
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"http://{host}:{port}/v1/traces");
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(resourceName))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                    {
                        exporterOptions.Endpoint = new Uri($"http://{host}:{port}/v1/metrics");
                        exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                        exporterOptions.ExportProcessorType = ExportProcessorType.Simple;
                        metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                    });
            })
            .WithLogging(logging =>
            {
                logging
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(resourceName))
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"http://{host}:{port}/v1/logs");
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
            });

        builder.Logging.AddFilter("System.Net.Http.HttpClient.OtlpMetricExporter", LogLevel.None);
        builder.Logging.AddFilter("System.Net.Http.HttpClient.OtlpLogExporter", LogLevel.None);
        builder.Logging.AddFilter("System.Net.Http.HttpClient.OtlpTraceExporter", LogLevel.None);
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Warning);
        builder.Logging.AddFilter("Polly", LogLevel.None);
    }
}
