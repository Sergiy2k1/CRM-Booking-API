using BookingHub.Application.Bookings.Common;

namespace BookingHub.Application.Bookings.ListBookings;

public sealed record ListBookingsResult(
    IReadOnlyCollection<BookingDetails> Items,
    int Page,
    int PageSize,
    int TotalCount);
