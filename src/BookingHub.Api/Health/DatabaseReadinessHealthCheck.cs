using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookingHub.Api.Health;

internal sealed class DatabaseReadinessHealthCheck
    : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseReadinessHealthCheck(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<BookingHubDbContext>();

            var canConnect =
                await dbContext.Database.CanConnectAsync(
                    cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy(
                    "PostgreSQL is reachable.")
                : HealthCheckResult.Unhealthy(
                    "PostgreSQL is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "PostgreSQL readiness check failed.",
                exception);
        }
    }
}
