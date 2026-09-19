using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Notifications;

public sealed class Notification : AggregateRoot
{
    public const int MaxTypeLength = 200;
    public const int MaxTitleLength = 200;
    public const int MaxMessageLength = 1000;

    private Notification(
        Guid id,
        Guid organizationId,
        Guid userId,
        Guid sourceMessageId,
        string type,
        string title,
        string message,
        Guid? relatedBookingId,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        UserId = userId;
        SourceMessageId = sourceMessageId;
        Type = type;
        Title = title;
        Message = message;
        RelatedBookingId = relatedBookingId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid SourceMessageId { get; private set; }

    public string Type { get; private set; }

    public string Title { get; private set; }

    public string Message { get; private set; }

    public Guid? RelatedBookingId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }

    public bool IsRead =>
        ReadAtUtc.HasValue;

    public static Notification Create(
        Guid id,
        Guid organizationId,
        Guid userId,
        Guid sourceMessageId,
        string type,
        string title,
        string message,
        Guid? relatedBookingId,
        DateTimeOffset createdAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            userId,
            nameof(userId));

        ValidateRelatedId(
            sourceMessageId,
            nameof(sourceMessageId));

        var normalizedType =
            NormalizeRequired(
                type,
                MaxTypeLength,
                nameof(type));

        var normalizedTitle =
            NormalizeRequired(
                title,
                MaxTitleLength,
                nameof(title));

        var normalizedMessage =
            NormalizeRequired(
                message,
                MaxMessageLength,
                nameof(message));

        if (relatedBookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Related booking id cannot be empty.",
                nameof(relatedBookingId));
        }

        return new Notification(
            id,
            organizationId,
            userId,
            sourceMessageId,
            normalizedType,
            normalizedTitle,
            normalizedMessage,
            relatedBookingId,
            createdAtUtc.ToUniversalTime());
    }

    public void MarkRead(
        DateTimeOffset readAtUtc)
    {
        if (ReadAtUtc.HasValue)
        {
            return;
        }

        ReadAtUtc =
            readAtUtc.ToUniversalTime();
    }

    private static void ValidateRelatedId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Related entity id cannot be empty.",
                parameterName);
        }
    }

    private static string NormalizeRequired(
        string value,
        int maxLength,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            value,
            parameterName);

        var normalized =
            value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }
}
