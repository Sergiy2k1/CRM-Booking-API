using BookingHub.Application.Abstractions.Reporting;
using BookingHub.Domain.Reporting;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Worker.Exports;

public sealed class BookingCsvExportWorker
    : BackgroundService
{
    private static readonly Action<ILogger, Guid, Exception?> LogExportFailure =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            new EventId(4001, nameof(LogExportFailure)),
            "Booking CSV export {ExportId} failed.");

    private static readonly Action<ILogger, Exception?> LogPollingFailure =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(4002, nameof(LogPollingFailure)),
            "Booking export polling failed. The worker will retry.");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCsvExportWorker> _logger;
    private readonly TimeSpan _pollInterval;
    private readonly int _maxAttempts;

    public BookingCsvExportWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BookingCsvExportWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _pollInterval =
            TimeSpan.FromSeconds(
                ReadPositiveInt(
                    configuration["BookingExports:PollIntervalSeconds"],
                    2));

        _maxAttempts =
            ReadPositiveInt(
                configuration["BookingExports:MaxAttempts"],
                3);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var exportJob =
                    await ClaimNextAsync(
                        stoppingToken);

                if (exportJob is null)
                {
                    await Task.Delay(
                        _pollInterval,
                        stoppingToken);

                    continue;
                }

                await ProcessAsync(
                    exportJob,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogPollingFailure(
                    _logger,
                    exception);

                await Task.Delay(
                    _pollInterval,
                    stoppingToken);
            }
        }
    }

    private async Task<ClaimedBookingExport?> ClaimNextAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<BookingHubDbContext>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var exportJob =
            await dbContext.BookingExportJobs
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM booking_export_jobs
                    WHERE "Status" = 1
                    ORDER BY "CreatedAtUtc", "Id"
                    FOR UPDATE SKIP LOCKED
                    LIMIT 1
                    """)
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (exportJob is null)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return null;
        }

        exportJob.MarkProcessing(
            DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new ClaimedBookingExport(
            exportJob.Id,
            exportJob.OrganizationId,
            exportJob.RequestedByUserId,
            exportJob.FromUtc,
            exportJob.ToUtc);
    }

    private async Task ProcessAsync(
        ClaimedBookingExport claimed,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var reader =
                scope.ServiceProvider
                    .GetRequiredService<IBookingExportReader>();

            var storage =
                scope.ServiceProvider
                    .GetRequiredService<IExportFileStorage>();

            var rows =
                await reader.ReadAsync(
                    claimed.OrganizationId,
                    claimed.FromUtc,
                    claimed.ToUtc,
                    cancellationToken);

            var csv =
                BookingCsvSerializer.Serialize(
                    rows);

            var fileName =
                $"bookings-{claimed.FromUtc:yyyyMMdd}-{claimed.ToUtc:yyyyMMdd}-{claimed.Id:N}.csv";

            var storageKey =
                $"{claimed.OrganizationId:N}/{claimed.RequestedByUserId:N}/{fileName}";

            await storage.SaveTextAsync(
                storageKey,
                csv,
                cancellationToken);

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<BookingHubDbContext>();

            var exportJob =
                await dbContext.BookingExportJobs
                    .SingleAsync(
                        x => x.Id == claimed.Id,
                        cancellationToken);

            exportJob.MarkCompleted(
                storageKey,
                fileName,
                DateTimeOffset.UtcNow);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogExportFailure(
                _logger,
                claimed.Id,
                exception);

            await RecordFailureAsync(
                claimed.Id,
                exception.Message,
                cancellationToken);
        }
    }

    private async Task RecordFailureAsync(
        Guid exportId,
        string error,
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<BookingHubDbContext>();

        var exportJob =
            await dbContext.BookingExportJobs
                .SingleAsync(
                    x => x.Id == exportId,
                    cancellationToken);

        exportJob.RecordFailure(
            error,
            _maxAttempts);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static int ReadPositiveInt(
        string? value,
        int fallback)
    {
        return int.TryParse(
                   value,
                   out var parsed) &&
               parsed > 0
            ? parsed
            : fallback;
    }

    private sealed record ClaimedBookingExport(
        Guid Id,
        Guid OrganizationId,
        Guid RequestedByUserId,
        DateTimeOffset FromUtc,
        DateTimeOffset ToUtc);
}
