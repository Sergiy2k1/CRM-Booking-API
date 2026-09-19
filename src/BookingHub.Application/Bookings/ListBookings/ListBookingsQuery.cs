using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.ListBookings;

public sealed record ListBookingsQuery(
    Guid OrganizationId,
    BookingStatus? Status,
    Guid? EmployeeId,
    DateTimeOffset? StartsFromUtc,
    DateTimeOffset? StartsBeforeUtc,
    int Page = 1,
    int PageSize = 20);
