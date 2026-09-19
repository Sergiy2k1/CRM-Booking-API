using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Abstractions.Messaging;
using BookingHub.Application.Bookings.Common;
using BookingHub.Application.Bookings.IntegrationEvents;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.ChangeBookingStatus;

public sealed class ChangeBookingStatusHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;

    public ChangeBookingStatusHandler(
        IBookingRepository bookingRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter)
    {
        _bookingRepository = bookingRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
    }

    public async Task<BookingDetails> HandleAsync(
        ChangeBookingStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var booking =
            await _bookingRepository.GetTrackedByOrganizationAndIdAsync(
                command.OrganizationId,
                command.BookingId,
                cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException(
                nameof(Booking),
                command.BookingId);
        }

        var utcNow = _clock.UtcNow;

        string eventName;

        switch (command.Transition)
        {
            case BookingTransition.Confirm:
                booking.Confirm(utcNow);
                eventName = BookingEventNames.Confirmed;
                break;

            case BookingTransition.Cancel:
                booking.Cancel(utcNow);
                eventName = BookingEventNames.Cancelled;
                break;

            case BookingTransition.Complete:
                booking.Complete(utcNow);
                eventName = BookingEventNames.Completed;
                break;

            case BookingTransition.MarkNoShow:
                booking.MarkNoShow(utcNow);
                eventName = BookingEventNames.NoShow;
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(command),
                    "Booking transition is invalid.");
        }

        await _outboxWriter.EnqueueAsync(
            eventName,
            BookingIntegrationEvent.From(
                booking,
                utcNow),
            utcNow,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return booking.ToDetails();
    }
}
