using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.CreateBooking;

public sealed record CreateBookingResult(
    Guid BookingId,
    BookingStatus Status,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    decimal PriceAmount,
    string Currency);
