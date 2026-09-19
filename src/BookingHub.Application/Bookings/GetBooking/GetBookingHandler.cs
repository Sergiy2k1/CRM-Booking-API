using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Bookings.Common;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.GetBooking;

public sealed class GetBookingHandler
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingHandler(
        IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingDetails> HandleAsync(
        GetBookingQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var booking =
            await _bookingRepository.GetByOrganizationAndIdAsync(
                query.OrganizationId,
                query.BookingId,
                cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException(
                nameof(Booking),
                query.BookingId);
        }

        return booking.ToDetails();
    }
}
