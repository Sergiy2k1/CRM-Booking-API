using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Notifications.Common;

namespace BookingHub.Application.Notifications.ListNotifications;

public sealed class ListNotificationsHandler
{
    public const int MaxPageSize = 100;

    private readonly INotificationRepository _notificationRepository;

    public ListNotificationsHandler(
        INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<ListNotificationsResult> HandleAsync(
        ListNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidateId(
            query.OrganizationId,
            nameof(query.OrganizationId));

        ValidateId(
            query.UserId,
            nameof(query.UserId));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            query.Page);

        if (query.PageSize < 1 ||
            query.PageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                $"Page size must be between 1 and {MaxPageSize}.");
        }

        var skip =
            (query.Page - 1) * query.PageSize;

        var notifications =
            await _notificationRepository.ListAsync(
                query.OrganizationId,
                query.UserId,
                query.IsRead,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _notificationRepository.CountAsync(
                query.OrganizationId,
                query.UserId,
                query.IsRead,
                cancellationToken);

        return new ListNotificationsResult(
            notifications
                .Select(x => x.ToDetails())
                .ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    private static void ValidateId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                parameterName);
        }
    }
}
