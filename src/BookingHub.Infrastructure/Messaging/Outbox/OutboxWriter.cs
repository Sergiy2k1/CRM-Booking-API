using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.Application.Abstractions.Messaging;
using BookingHub.Infrastructure.Persistence;

namespace BookingHub.Infrastructure.Messaging.Outbox;

internal sealed class OutboxWriter
    : IOutboxWriter
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private readonly BookingHubDbContext _dbContext;

    public OutboxWriter(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnqueueAsync<T>(
        string type,
        T payload,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentNullException.ThrowIfNull(payload);

        var serializedPayload =
            JsonSerializer.Serialize(
                payload,
                SerializerOptions);

        var message =
            OutboxMessage.Create(
                Guid.NewGuid(),
                type,
                serializedPayload,
                occurredAtUtc);

        await _dbContext.OutboxMessages.AddAsync(
            message,
            cancellationToken);
    }
}
