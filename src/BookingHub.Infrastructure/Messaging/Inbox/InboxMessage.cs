namespace BookingHub.Infrastructure.Messaging.Inbox;

public sealed class InboxMessage
{
    public const int MaxConsumerLength = 200;

    private InboxMessage()
    {
        Consumer = string.Empty;
    }

    private InboxMessage(
        string consumer,
        Guid messageId,
        DateTimeOffset processedAtUtc)
    {
        Consumer = consumer;
        MessageId = messageId;
        ProcessedAtUtc = processedAtUtc.ToUniversalTime();
    }

    public string Consumer { get; private set; }

    public Guid MessageId { get; private set; }

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static InboxMessage Create(
        string consumer,
        Guid messageId,
        DateTimeOffset processedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            consumer);

        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Inbox message id cannot be empty.",
                nameof(messageId));
        }

        var normalizedConsumer =
            consumer.Trim();

        if (normalizedConsumer.Length > MaxConsumerLength)
        {
            throw new ArgumentException(
                $"Inbox consumer cannot exceed {MaxConsumerLength} characters.",
                nameof(consumer));
        }

        return new InboxMessage(
            normalizedConsumer,
            messageId,
            processedAtUtc);
    }
}
