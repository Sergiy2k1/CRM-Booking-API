namespace BookingHub.Application.Bookings.IntegrationEvents;

public static class BookingEventNames
{
    public const string Created = "booking.created";

    public const string Confirmed = "booking.confirmed";

    public const string Rescheduled = "booking.rescheduled";

    public const string Cancelled = "booking.cancelled";

    public const string Completed = "booking.completed";

    public const string NoShow = "booking.no_show";
}
