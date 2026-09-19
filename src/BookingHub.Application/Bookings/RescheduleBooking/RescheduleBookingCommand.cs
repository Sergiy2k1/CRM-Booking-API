namespace BookingHub.Application.Bookings.RescheduleBooking;

public sealed record RescheduleBookingCommand(
    Guid OrganizationId,
    Guid BookingId,
    DateTimeOffset StartsAtUtc);
