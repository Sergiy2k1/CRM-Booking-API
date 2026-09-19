namespace BookingHub.Api.Contracts.Bookings;

public sealed record CreateBookingRequest(
    Guid CustomerId,
    Guid EmployeeId,
    Guid ServiceId,
    DateTimeOffset StartsAtUtc,
    string? Notes);
