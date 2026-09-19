using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace BookingHub.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqPublisher
    : IAsyncDisposable
{
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _virtualHost;
    private readonly string _exchange;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(
        IConfiguration configuration)
    {
        _hostName =
            configuration["RabbitMq:HostName"]
            ?? "localhost";

        _port =
            int.TryParse(
                configuration["RabbitMq:Port"],
                out var port)
                ? port
                : 5672;

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
    }

    public async Task PublishAsync(
        Guid messageId,
        string routingKey,
        string payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        var channel =
            await GetChannelAsync(
                cancellationToken);

        var properties =
            new BasicProperties
            {
                Persistent = true,
                MessageId = messageId.ToString(),
                ContentType = "application/json",
                Type = routingKey
            };

        var body =
            Encoding.UTF8.GetBytes(payload);

        await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task<IChannel> GetChannelAsync(
        CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

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

        _connection =
            await factory.CreateConnectionAsync(
                cancellationToken);

        _channel =
            await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);


        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
