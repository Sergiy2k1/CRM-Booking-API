using BookingHub.Infrastructure.Messaging.Outbox;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using BookingHub.Infrastructure.Messaging.RabbitMq;
using BookingHub.Worker.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Worker.Outbox;

public sealed class OutboxPublisherWorker
    : BackgroundService
{
    private static readonly Action<ILogger, Guid, string, int, Exception?> LogPublishFailure =
        LoggerMessage.Define<Guid, string, int>(
            LogLevel.Error,
            new EventId(1001, nameof(LogPublishFailure)),
            "Failed to publish outbox message {OutboxMessageId} of type {OutboxMessageType}. Attempt {AttemptCount}.");

    private static readonly Action<ILogger, Exception?> LogProcessingFailure =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1002, nameof(LogProcessingFailure)),
            "Outbox polling failed. The worker will retry.");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly int _batchSize;
    private readonly int _maxAttempts;
    private readonly TimeSpan _pollInterval;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        RabbitMqPublisher publisher,
        IConfiguration configuration,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;

        _batchSize =
            ReadPositiveInt(
                configuration["Outbox:BatchSize"],
                50);

        _maxAttempts =
            ReadPositiveInt(
                configuration["Outbox:MaxAttempts"],
                10);

        _pollInterval =
            TimeSpan.FromSeconds(
                ReadPositiveInt(
                    configuration["Outbox:PollIntervalSeconds"],
                    2));
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var publishedCount =
                    await PublishPendingAsync(
                        stoppingToken);

                if (publishedCount == 0)
                {
                    await Task.Delay(
                        _pollInterval,
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogProcessingFailure(
                    _logger,
                    exception);

                await Task.Delay(
                    _pollInterval,
                    stoppingToken);
            }
        }
    }

    private async Task<int> PublishPendingAsync(
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

        var messages =
            await dbContext.OutboxMessages
                .FromSqlInterpolated(
                    $"""
                    SELECT *
                    FROM outbox_messages
                    WHERE "ProcessedAtUtc" IS NULL
                      AND "AttemptCount" < {_maxAttempts}
                    ORDER BY "OccurredAtUtc", "Id"
                    FOR UPDATE SKIP LOCKED
                    LIMIT {_batchSize}
                    """)
                .ToListAsync(
                    cancellationToken);

        foreach (var message in messages)
        {
            using var activity =
                WorkerTelemetry.ActivitySource.StartActivity(
                    "outbox.publish",
                    ActivityKind.Producer);

            activity?.SetTag(
                "bookinghub.outbox.message_id",
                message.Id);

            activity?.SetTag(
                "bookinghub.outbox.message_type",
                message.Type);

            try
            {
                await _publisher.PublishAsync(
                    message.Id,
                    message.Type,
                    message.Payload,
                    cancellationToken);

                message.MarkProcessed(
                    DateTimeOffset.UtcNow);

                WorkerTelemetry.OutboxPublished.Add(1);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                message.RecordFailure(
                    exception.Message);

                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    exception.Message);

                WorkerTelemetry.OutboxFailed.Add(1);

                LogPublishFailure(
                    _logger,
                    message.Id,
                    message.Type,
                    message.AttemptCount,
                    exception);
            }

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        await transaction.CommitAsync(
            cancellationToken);

        return messages.Count;
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
}
