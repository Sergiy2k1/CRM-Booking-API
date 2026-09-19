using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Bookings.Common;

internal static class BookingMappings
{
    public static BookingDetails ToDetails(
        this Booking booking)
    {
        return new BookingDetails(
            booking.Id,
            booking.OrganizationId,
            booking.CustomerId,
            booking.EmployeeId,
            booking.ServiceId,
            booking.StartsAtUtc,
            booking.EndsAtUtc,
            booking.PriceAmount,
            booking.Currency,
            booking.Notes,
            booking.Status,
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc,
            booking.CancelledAtUtc,
            booking.CompletedAtUtc,
            booking.NoShowAtUtc);
    }
}
