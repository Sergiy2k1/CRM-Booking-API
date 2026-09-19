using BookingHub.Application.Bookings.IntegrationEvents;

namespace BookingHub.Api.Realtime;

internal static class BookingRealtimeClientMethods
{
    public static string FromEventName(string eventName)
    {
        return eventName switch
        {
            BookingEventNames.Created => "bookingCreated",
            BookingEventNames.Confirmed => "bookingConfirmed",
            BookingEventNames.Rescheduled => "bookingRescheduled",
            BookingEventNames.Cancelled => "bookingCancelled",
            BookingEventNames.Completed => "bookingCompleted",
            BookingEventNames.NoShow => "bookingNoShow",
            _ => throw new ArgumentOutOfRangeException(
                nameof(eventName),
                eventName,
                "Unsupported booking event name.")
        };
    }
}
