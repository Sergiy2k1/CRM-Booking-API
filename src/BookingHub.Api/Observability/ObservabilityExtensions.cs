using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BookingHub.Api.Observability;

internal static class ObservabilityExtensions
{
    public static IServiceCollection AddApiObservability(
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
                        serviceName: "BookingHub.Api"))
            .WithTracing(
                tracing =>
                    tracing
                        .AddAspNetCoreInstrumentation(
                            options =>
                            {
                                options.RecordException = true;

                                options.Filter =
                                    context =>
                                        !context.Request.Path
                                            .StartsWithSegments(
                                                "/health");
                            })
                        .AddHttpClientInstrumentation(
                            options =>
                                options.RecordException = true)
                        .AddOtlpExporter(
                            options =>
                                options.Endpoint = endpoint))
            .WithMetrics(
                metrics =>
                    metrics
                        .AddAspNetCoreInstrumentation()
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
