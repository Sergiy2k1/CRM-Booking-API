using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.IntegrationEvents;

public sealed record BookingIntegrationEvent(
    Guid BookingId,
    Guid OrganizationId,
    Guid CustomerId,
    Guid EmployeeId,
    Guid ServiceId,
    BookingStatus Status,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    decimal PriceAmount,
    string Currency,
    DateTimeOffset OccurredAtUtc)
{
    public static BookingIntegrationEvent From(
        Booking booking,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(booking);

        return new BookingIntegrationEvent(
            booking.Id,
            booking.OrganizationId,
            booking.CustomerId,
            booking.EmployeeId,
            booking.ServiceId,
            booking.Status,
            booking.StartsAtUtc,
            booking.EndsAtUtc,
            booking.PriceAmount,
            booking.Currency,
            occurredAtUtc.ToUniversalTime());
    }
}
