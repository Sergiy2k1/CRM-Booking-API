using BookingHub.Domain.Notifications;

namespace BookingHub.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    Task<IReadOnlyCollection<Notification>> ListAsync(
        Guid organizationId,
        Guid userId,
        bool? isRead,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid organizationId,
        Guid userId,
        bool? isRead,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetTrackedAsync(
        Guid organizationId,
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyCollection<Notification> notifications,
        CancellationToken cancellationToken = default);
}
