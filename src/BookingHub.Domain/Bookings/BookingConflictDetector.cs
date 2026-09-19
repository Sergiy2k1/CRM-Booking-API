namespace BookingHub.Domain.Bookings;

public static class BookingConflictDetector
{
    public static bool HasConflict(
        IEnumerable<Booking> existingBookings,
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? bookingIdToExclude = null)
    {
        ArgumentNullException.ThrowIfNull(existingBookings);

        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            employeeId,
            nameof(employeeId));

        var utcStartsAt = startsAtUtc.ToUniversalTime();
        var utcEndsAt = endsAtUtc.ToUniversalTime();

        ValidateTimeRange(
            utcStartsAt,
            utcEndsAt);

        foreach (var booking in existingBookings)
        {
            if (bookingIdToExclude.HasValue &&
                booking.Id == bookingIdToExclude.Value)
            {
                continue;
            }

            if (booking.OrganizationId != organizationId ||
                booking.EmployeeId != employeeId)
            {
                continue;
            }

            if (!BlocksTimeSlot(booking.Status))
            {
                continue;
            }

            if (TimeRangesOverlap(
                    booking.StartsAtUtc,
                    booking.EndsAtUtc,
                    utcStartsAt,
                    utcEndsAt))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TimeRangesOverlap(
        DateTimeOffset firstStartsAtUtc,
        DateTimeOffset firstEndsAtUtc,
        DateTimeOffset secondStartsAtUtc,
        DateTimeOffset secondEndsAtUtc)
    {
        var firstStart = firstStartsAtUtc.ToUniversalTime();
        var firstEnd = firstEndsAtUtc.ToUniversalTime();
        var secondStart = secondStartsAtUtc.ToUniversalTime();
        var secondEnd = secondEndsAtUtc.ToUniversalTime();

        ValidateTimeRange(
            firstStart,
            firstEnd);

        ValidateTimeRange(
            secondStart,
            secondEnd);

        return firstStart < secondEnd &&
               secondStart < firstEnd;
    }

    private static bool BlocksTimeSlot(BookingStatus status)
    {
        return status is BookingStatus.Pending or
            BookingStatus.Confirmed;
    }

    private static void ValidateRelatedId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Related entity id cannot be empty.",
                parameterName);
        }
    }

    private static void ValidateTimeRange(
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new ArgumentException(
                "Booking end must be later than start.",
                nameof(endsAtUtc));
        }
    }
}
