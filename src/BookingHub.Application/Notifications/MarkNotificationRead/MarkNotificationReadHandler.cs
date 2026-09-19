using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Notifications.Common;
using BookingHub.Domain.Notifications;

namespace BookingHub.Application.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadHandler
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadHandler(
        INotificationRepository notificationRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationDetails> HandleAsync(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var notification =
            await _notificationRepository.GetTrackedAsync(
                command.OrganizationId,
                command.UserId,
                command.NotificationId,
                cancellationToken);

        if (notification is null)
        {
            throw new EntityNotFoundException(
                nameof(Notification),
                command.NotificationId);
        }

        notification.MarkRead(
            _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return notification.ToDetails();
    }
}
