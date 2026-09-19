using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Bookings.Common;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.ChangeBookingStatus;

public sealed class ChangeBookingStatusHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeBookingStatusHandler(
        IBookingRepository bookingRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
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

        switch (command.Transition)
        {
            case BookingTransition.Confirm:
                booking.Confirm(utcNow);
                break;

            case BookingTransition.Cancel:
                booking.Cancel(utcNow);
                break;

            case BookingTransition.Complete:
                booking.Complete(utcNow);
                break;

            case BookingTransition.MarkNoShow:
                booking.MarkNoShow(utcNow);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(command),
                    "Booking transition is invalid.");
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return booking.ToDetails();
    }
}
