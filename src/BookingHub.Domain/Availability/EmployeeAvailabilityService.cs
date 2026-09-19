using BookingHub.Domain.Bookings;
using BookingHub.Domain.Employees;

namespace BookingHub.Domain.Availability;

public static class EmployeeAvailabilityService
{
    public static EmployeeAvailabilityStatus Check(
        IEnumerable<EmployeeWorkingHours> workingHours,
        IEnumerable<EmployeeTimeOff> timeOffPeriods,
        IEnumerable<Booking> existingBookings,
        Guid organizationId,
        Guid employeeId,
        TimeZoneInfo timeZone,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? bookingIdToExclude = null)
    {
        ArgumentNullException.ThrowIfNull(workingHours);
        ArgumentNullException.ThrowIfNull(timeOffPeriods);
        ArgumentNullException.ThrowIfNull(existingBookings);
        ArgumentNullException.ThrowIfNull(timeZone);

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

        if (!IsWithinWorkingHours(
                workingHours,
                organizationId,
                employeeId,
                timeZone,
                utcStartsAt,
                utcEndsAt))
        {
            return EmployeeAvailabilityStatus.OutsideWorkingHours;
        }

        if (HasActiveTimeOffConflict(
                timeOffPeriods,
                organizationId,
                employeeId,
                utcStartsAt,
                utcEndsAt))
        {
            return EmployeeAvailabilityStatus.TimeOff;
        }

        if (BookingConflictDetector.HasConflict(
                existingBookings,
                organizationId,
                employeeId,
                utcStartsAt,
                utcEndsAt,
                bookingIdToExclude))
        {
            return EmployeeAvailabilityStatus.BookingConflict;
        }

        return EmployeeAvailabilityStatus.Available;
    }

    private static bool IsWithinWorkingHours(
        IEnumerable<EmployeeWorkingHours> workingHours,
        Guid organizationId,
        Guid employeeId,
        TimeZoneInfo timeZone,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        var localStartsAt =
            TimeZoneInfo.ConvertTime(
                startsAtUtc,
                timeZone);

        var localEndsAt =
            TimeZoneInfo.ConvertTime(
                endsAtUtc,
                timeZone);

        if (localStartsAt.Date != localEndsAt.Date)
        {
            return false;
        }

        var localStartTime =
            TimeOnly.FromDateTime(localStartsAt.DateTime);

        var localEndTime =
            TimeOnly.FromDateTime(localEndsAt.DateTime);

        foreach (var workingPeriod in workingHours)
        {
            if (workingPeriod.OrganizationId != organizationId ||
                workingPeriod.EmployeeId != employeeId ||
                workingPeriod.DayOfWeek != localStartsAt.DayOfWeek)
            {
                continue;
            }

            if (workingPeriod.StartTime <= localStartTime &&
                localEndTime <= workingPeriod.EndTime)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasActiveTimeOffConflict(
        IEnumerable<EmployeeTimeOff> timeOffPeriods,
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        foreach (var timeOff in timeOffPeriods)
        {
            if (timeOff.OrganizationId != organizationId ||
                timeOff.EmployeeId != employeeId ||
                timeOff.Status != EmployeeTimeOffStatus.Active)
            {
                continue;
            }

            if (BookingConflictDetector.TimeRangesOverlap(
                    timeOff.StartsAtUtc,
                    timeOff.EndsAtUtc,
                    startsAtUtc,
                    endsAtUtc))
            {
                return true;
            }
        }

        return false;
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
                "Availability end must be later than start.",
                nameof(endsAtUtc));
        }
    }
}
