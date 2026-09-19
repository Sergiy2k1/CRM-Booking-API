using BookingHub.Domain.Bookings;

namespace BookingHub.Api.Contracts.Bookings;

public sealed record CreateBookingResponse(
    Guid BookingId,
    BookingStatus Status,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    decimal PriceAmount,
    string Currency);
