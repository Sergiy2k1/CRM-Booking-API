namespace BookingHub.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    Task EnqueueAsync<T>(
        string type,
        T payload,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken = default)
        where T : notnull;
}
