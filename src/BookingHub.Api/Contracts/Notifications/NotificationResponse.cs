namespace BookingHub.Api.Contracts.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Message,
    Guid? RelatedBookingId,
    DateTimeOffset CreatedAtUtc,
    bool IsRead,
    DateTimeOffset? ReadAtUtc);
