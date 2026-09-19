using BookingHub.Application.Notifications.Common;

namespace BookingHub.Application.Notifications.ListNotifications;

public sealed record ListNotificationsResult(
    IReadOnlyCollection<NotificationDetails> Items,
    int Page,
    int PageSize,
    int TotalCount);
