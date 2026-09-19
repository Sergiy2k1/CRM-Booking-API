namespace BookingHub.Application.Bookings.GetBooking;

public sealed record GetBookingQuery(
    Guid OrganizationId,
    Guid BookingId);
