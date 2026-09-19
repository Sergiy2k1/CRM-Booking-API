using BookingHub.Infrastructure.Messaging.Outbox;
using BookingHub.Infrastructure.Messaging.RabbitMq;
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

        var repository =
            scope.ServiceProvider
                .GetRequiredService<OutboxRepository>();

        var messages =
            await repository.GetPendingAsync(
                _batchSize,
                _maxAttempts,
                cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublishAsync(
                    message.Id,
                    message.Type,
                    message.Payload,
                    cancellationToken);

                message.MarkProcessed(
                    DateTimeOffset.UtcNow);
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

                LogPublishFailure(
                    _logger,
                    message.Id,
                    message.Type,
                    message.AttemptCount,
                    exception);
            }

            await repository.SaveChangesAsync(
                cancellationToken);
        }

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
