using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.Application.Bookings.IntegrationEvents;
using BookingHub.Domain.Notifications;
using BookingHub.Domain.Organizations;
using BookingHub.Infrastructure.Messaging.Inbox;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BookingHub.Worker.Notifications;

public sealed class BookingNotificationConsumer
    : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private static readonly Action<ILogger, Exception?> LogConnectionFailure =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(3001, nameof(LogConnectionFailure)),
            "RabbitMQ notification consumer is unavailable and will retry.");

    private static readonly Action<ILogger, string, ulong, Exception?> LogMessageFailure =
        LoggerMessage.Define<string, ulong>(
            LogLevel.Error,
            new EventId(3002, nameof(LogMessageFailure)),
            "Failed to process booking notification event {RoutingKey} with delivery tag {DeliveryTag}.");

    private const string ConsumerName = "bookinghub.notifications";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingNotificationConsumer> _logger;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _virtualHost;
    private readonly string _exchange;
    private readonly string _queue;
    private readonly string _bindingKey;
    private readonly TimeSpan _reconnectDelay;

    public BookingNotificationConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BookingNotificationConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _hostName =
            configuration["RabbitMq:HostName"]
            ?? "localhost";

        _port =
            ReadPositiveInt(
                configuration["RabbitMq:Port"],
                5672);

        _userName =
            configuration["RabbitMq:UserName"]
            ?? "guest";

        _password =
            configuration["RabbitMq:Password"]
            ?? "guest";

        _virtualHost =
            configuration["RabbitMq:VirtualHost"]
            ?? "/";

        _exchange =
            configuration["RabbitMq:Exchange"]
            ?? "bookinghub.events";

        _queue =
            configuration["RabbitMq:NotificationQueue"]
            ?? "bookinghub.notifications";

        _bindingKey =
            configuration["RabbitMq:NotificationBindingKey"]
            ?? "booking.#";

        _reconnectDelay =
            TimeSpan.FromSeconds(
                ReadPositiveInt(
                    configuration["RabbitMq:ReconnectSeconds"],
                    5));
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogConnectionFailure(
                    _logger,
                    exception);

                await Task.Delay(
                    _reconnectDelay,
                    stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(
        CancellationToken cancellationToken)
    {
        var factory =
            new ConnectionFactory
            {
                HostName = _hostName,
                Port = _port,
                UserName = _userName,
                Password = _password,
                VirtualHost = _virtualHost,
                AutomaticRecoveryEnabled = true
            };

        await using var connection =
            await factory.CreateConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _queue,
            exchange: _exchange,
            routingKey: _bindingKey,
            arguments: null,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 20,
            global: false,
            cancellationToken: cancellationToken);

        var consumer =
            new AsyncEventingBasicConsumer(
                channel);

        consumer.ReceivedAsync +=
            async (_, delivery) =>
            {
                try
                {
                    await ProcessAsync(
                        delivery,
                        cancellationToken);

                    await channel.BasicAckAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    LogMessageFailure(
                        _logger,
                        delivery.RoutingKey,
                        delivery.DeliveryTag,
                        exception);

                    await Task.Delay(
                        TimeSpan.FromSeconds(1),
                        cancellationToken);

                    await channel.BasicNackAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken);
                }
            };

        await channel.BasicConsumeAsync(
            queue: _queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        await Task.Delay(
            Timeout.InfiniteTimeSpan,
            cancellationToken);
    }

    private async Task ProcessAsync(
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(
                delivery.BasicProperties.MessageId,
                out var messageId))
        {
            throw new InvalidOperationException(
                "RabbitMQ booking event does not contain a valid MessageId.");
        }

        var payload =
            Encoding.UTF8.GetString(
                delivery.Body.Span);

        var bookingEvent =
            JsonSerializer.Deserialize<BookingIntegrationEvent>(
                payload,
                SerializerOptions)
            ?? throw new JsonException(
                "Booking integration event payload is empty.");

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<BookingHubDbContext>();

        var alreadyProcessed =
            await dbContext.InboxMessages
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Consumer == ConsumerName &&
                        x.MessageId == messageId,
                    cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        var members =
            await dbContext.OrganizationMembers
                .AsNoTracking()
                .Where(
                    x =>
                        x.OrganizationId == bookingEvent.OrganizationId &&
                        x.Status == OrganizationMemberStatus.Active)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

        var content =
            BookingNotificationMessageFactory.Create(
                delivery.RoutingKey,
                bookingEvent);

        var notifications =
            members
                .Select(
                    userId =>
                        Notification.Create(
                            Guid.NewGuid(),
                            bookingEvent.OrganizationId,
                            userId,
                            messageId,
                            delivery.RoutingKey,
                            content.Title,
                            content.Message,
                            bookingEvent.BookingId,
                            bookingEvent.OccurredAtUtc))
                .ToArray();

        if (notifications.Length > 0)
        {
            await dbContext.Notifications.AddRangeAsync(
                notifications,
                cancellationToken);
        }

        await dbContext.InboxMessages.AddAsync(
            InboxMessage.Create(
                ConsumerName,
                messageId,
                DateTimeOffset.UtcNow),
            cancellationToken);

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
}
