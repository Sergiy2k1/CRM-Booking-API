namespace BookingHub.Api.Contracts.Notifications;

public sealed record NotificationListResponse(
    IReadOnlyCollection<NotificationResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
