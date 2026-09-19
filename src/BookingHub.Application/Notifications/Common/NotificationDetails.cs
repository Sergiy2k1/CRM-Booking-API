namespace BookingHub.Application.Notifications.Common;

public sealed record NotificationDetails(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    Guid? RelatedBookingId,
    DateTimeOffset CreatedAtUtc,
    bool IsRead,
    DateTimeOffset? ReadAtUtc);
