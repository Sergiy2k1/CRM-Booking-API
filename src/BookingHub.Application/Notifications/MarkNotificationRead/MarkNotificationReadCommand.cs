namespace BookingHub.Application.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(
    Guid OrganizationId,
    Guid UserId,
    Guid NotificationId);
