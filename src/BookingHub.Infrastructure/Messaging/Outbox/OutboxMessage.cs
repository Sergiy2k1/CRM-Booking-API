namespace BookingHub.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
    public const int MaxTypeLength = 200;
    public const int MaxErrorLength = 2000;

    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    private OutboxMessage(
        Guid id,
        string type,
        string payload,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    public static OutboxMessage Create(
        Guid id,
        string type,
        string payload,
        DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Outbox message id cannot be empty.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        var normalizedType = type.Trim();

        if (normalizedType.Length > MaxTypeLength)
        {
            throw new ArgumentException(
                $"Outbox message type cannot exceed {MaxTypeLength} characters.",
                nameof(type));
        }

        return new OutboxMessage(
            id,
            normalizedType,
            payload,
            occurredAtUtc);
    }

    public void MarkProcessed(
        DateTimeOffset processedAtUtc)
    {
        AttemptCount++;
        ProcessedAtUtc =
            processedAtUtc.ToUniversalTime();

        LastError = null;
    }

    public void RecordFailure(string error)
    {
        AttemptCount++;

        LastError =
            string.IsNullOrWhiteSpace(error)
                ? "Unknown publishing error."
                : error.Trim()[..Math.Min(
                    error.Trim().Length,
                    MaxErrorLength)];
    }
}
