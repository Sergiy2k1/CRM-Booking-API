using BookingHub.Domain.Notifications;

namespace BookingHub.Application.Notifications.Common;

internal static class NotificationMappings
{
    public static NotificationDetails ToDetails(
        this Notification notification)
    {
        return new NotificationDetails(
            notification.Id,
            notification.OrganizationId,
            notification.UserId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.RelatedBookingId,
            notification.CreatedAtUtc,
            notification.IsRead,
            notification.ReadAtUtc);
    }
}
