namespace BookingHub.Api.Contracts.Bookings;

public sealed record RescheduleBookingRequest(
    DateTimeOffset StartsAtUtc);
