using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository
    : INotificationRepository
{
    private readonly BookingHubDbContext _dbContext;

    public NotificationRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Notification>> ListAsync(
        Guid organizationId,
        Guid userId,
        bool? isRead,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query =
            BuildUserQuery(
                    organizationId,
                    userId,
                    isRead)
                .AsNoTracking();

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid organizationId,
        Guid userId,
        bool? isRead,
        CancellationToken cancellationToken = default)
    {
        return BuildUserQuery(
                organizationId,
                userId,
                isRead)
            .CountAsync(cancellationToken);
    }

    public Task<Notification?> GetTrackedAsync(
        Guid organizationId,
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == userId &&
                    x.Id == notificationId,
                cancellationToken);
    }

    public async Task AddRangeAsync(
        IReadOnlyCollection<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            notifications);

        await _dbContext.Notifications.AddRangeAsync(
            notifications,
            cancellationToken);
    }

    private IQueryable<Notification> BuildUserQuery(
        Guid organizationId,
        Guid userId,
        bool? isRead)
    {
        var query =
            _dbContext.Notifications.Where(
                x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == userId);

        if (isRead.HasValue)
        {
            query =
                isRead.Value
                    ? query.Where(x => x.ReadAtUtc != null)
                    : query.Where(x => x.ReadAtUtc == null);
        }

        return query;
    }
}
