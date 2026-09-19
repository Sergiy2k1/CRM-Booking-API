using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BookingHub.Worker.Observability;

internal static class ObservabilityExtensions
{
    public static IServiceCollection AddWorkerObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>(
                "OpenTelemetry:Enabled"))
        {
            return services;
        }

        var endpoint =
            GetOtlpEndpoint(
                configuration);

        services
            .AddOpenTelemetry()
            .ConfigureResource(
                resource =>
                    resource.AddService(
                        serviceName: "BookingHub.Worker"))
            .WithTracing(
                tracing =>
                    tracing
                        .AddSource(
                            WorkerTelemetry.ActivitySourceName)
                        .AddHttpClientInstrumentation(
                            options =>
                                options.RecordException = true)
                        .AddOtlpExporter(
                            options =>
                                options.Endpoint = endpoint))
            .WithMetrics(
                metrics =>
                    metrics
                        .AddMeter(
                            WorkerTelemetry.MeterName)
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(
                            options =>
                                options.Endpoint = endpoint));

        return services;
    }

    private static Uri GetOtlpEndpoint(
        IConfiguration configuration)
    {
        var value =
            configuration["OpenTelemetry:OtlpEndpoint"];

        if (!Uri.TryCreate(
                value,
                UriKind.Absolute,
                out var endpoint))
        {
            throw new InvalidOperationException(
                "OpenTelemetry OTLP endpoint is invalid.");
        }

        return endpoint;
    }
}

internal static class WorkerTelemetry
{
    public const string ActivitySourceName =
        "BookingHub.Worker";

    public const string MeterName =
        "BookingHub.Worker";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);

    private static readonly Meter Meter =
        new(MeterName);

    public static readonly Counter<long> OutboxPublished =
        Meter.CreateCounter<long>(
            "bookinghub.outbox.published");

    public static readonly Counter<long> OutboxFailed =
        Meter.CreateCounter<long>(
            "bookinghub.outbox.failed");

    public static readonly Counter<long> ExportsCompleted =
        Meter.CreateCounter<long>(
            "bookinghub.exports.completed");

    public static readonly Counter<long> ExportsFailed =
        Meter.CreateCounter<long>(
            "bookinghub.exports.failed");
}
