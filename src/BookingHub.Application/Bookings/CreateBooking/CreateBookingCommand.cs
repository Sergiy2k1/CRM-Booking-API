namespace BookingHub.Application.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    Guid OrganizationId,
    Guid CustomerId,
    Guid EmployeeId,
    Guid ServiceId,
    DateTimeOffset StartsAtUtc,
    string? Notes);
