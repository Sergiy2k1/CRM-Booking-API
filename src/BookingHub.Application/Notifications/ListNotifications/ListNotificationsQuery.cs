namespace BookingHub.Application.Notifications.ListNotifications;

public sealed record ListNotificationsQuery(
    Guid OrganizationId,
    Guid UserId,
    bool? IsRead,
    int Page = 1,
    int PageSize = 20);
