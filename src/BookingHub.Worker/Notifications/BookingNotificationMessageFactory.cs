using BookingHub.Application.Bookings.IntegrationEvents;

namespace BookingHub.Worker.Notifications;

internal static class BookingNotificationMessageFactory
{
    public static NotificationContent Create(
        string eventName,
        BookingIntegrationEvent bookingEvent)
    {
        ArgumentNullException.ThrowIfNull(
            bookingEvent);

        return eventName switch
        {
            BookingEventNames.Created =>
                new NotificationContent(
                    "Booking created",
                    $"Booking {bookingEvent.BookingId} was created."),

            BookingEventNames.Confirmed =>
                new NotificationContent(
                    "Booking confirmed",
                    $"Booking {bookingEvent.BookingId} was confirmed."),

            BookingEventNames.Rescheduled =>
                new NotificationContent(
                    "Booking rescheduled",
                    $"Booking {bookingEvent.BookingId} was rescheduled."),

            BookingEventNames.Cancelled =>
                new NotificationContent(
                    "Booking cancelled",
                    $"Booking {bookingEvent.BookingId} was cancelled."),

            BookingEventNames.Completed =>
                new NotificationContent(
                    "Booking completed",
                    $"Booking {bookingEvent.BookingId} was completed."),

            BookingEventNames.NoShow =>
                new NotificationContent(
                    "Booking no-show",
                    $"Booking {bookingEvent.BookingId} was marked as no-show."),

            _ => throw new ArgumentOutOfRangeException(
                nameof(eventName),
                eventName,
                "Unsupported booking event name.")
        };
    }

    internal sealed record NotificationContent(
        string Title,
        string Message);
}
