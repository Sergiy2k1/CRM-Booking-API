using BookingHub.Domain.Availability;

namespace BookingHub.Application.Bookings.CreateBooking;

public sealed class BookingUnavailableException : Exception
{
    public BookingUnavailableException(
        EmployeeAvailabilityStatus availabilityStatus)
        : base($"Requested booking slot is unavailable: {availabilityStatus}.")
    {
        AvailabilityStatus = availabilityStatus;
    }

    public EmployeeAvailabilityStatus AvailabilityStatus { get; }
}
