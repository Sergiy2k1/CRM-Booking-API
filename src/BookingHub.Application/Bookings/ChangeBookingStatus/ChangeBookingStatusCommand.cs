namespace BookingHub.Application.Bookings.ChangeBookingStatus;

public sealed record ChangeBookingStatusCommand(
    Guid OrganizationId,
    Guid BookingId,
    BookingTransition Transition);
