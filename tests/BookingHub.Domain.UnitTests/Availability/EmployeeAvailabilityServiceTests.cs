using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Employees;
using Xunit;

namespace BookingHub.Domain.UnitTests.Availability;

public sealed class EmployeeAvailabilityServiceTests
{
    private static readonly TimeZoneInfo TestTimeZone =
        TimeZoneInfo.CreateCustomTimeZone(
            "BookingHub-Test-UTC-3",
            TimeSpan.FromHours(3),
            "BookingHub Test UTC+3",
            "BookingHub Test UTC+3");

    [Fact]
    public void CheckInsideWorkingHoursWithoutConflictsShouldReturnAvailable()
    {
        var context = CreateContext();

        var result = EmployeeAvailabilityService.Check(
            [CreateWorkingHours(context)],
            [],
            [],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.Available,
            result);
    }

    [Fact]
    public void CheckBeforeWorkingHoursShouldReturnOutsideWorkingHours()
    {
        var context = CreateContext();

        var result = EmployeeAvailabilityService.Check(
            [CreateWorkingHours(context)],
            [],
            [],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc.AddHours(-2),
            context.StartsAtUtc.AddHours(-1));

        Assert.Equal(
            EmployeeAvailabilityStatus.OutsideWorkingHours,
            result);
    }

    [Fact]
    public void CheckWithoutWorkingHoursForDayShouldReturnOutsideWorkingHours()
    {
        var context = CreateContext();

        var tuesdayWorkingHours =
            EmployeeWorkingHours.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.EmployeeId,
                DayOfWeek.Tuesday,
                new TimeOnly(9, 0),
                new TimeOnly(18, 0));

        var result = EmployeeAvailabilityService.Check(
            [tuesdayWorkingHours],
            [],
            [],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.OutsideWorkingHours,
            result);
    }

    [Fact]
    public void CheckWithActiveTimeOffOverlapShouldReturnTimeOff()
    {
        var context = CreateContext();

        var timeOff = EmployeeTimeOff.Create(
            Guid.NewGuid(),
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90),
            "Appointment",
            DateTimeOffset.UtcNow);

        var result = EmployeeAvailabilityService.Check(
            [CreateWorkingHours(context)],
            [timeOff],
            [],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.TimeOff,
            result);
    }

    [Fact]
    public void CheckWithCancelledTimeOffShouldReturnAvailable()
    {
        var context = CreateContext();

        var timeOff = EmployeeTimeOff.Create(
            Guid.NewGuid(),
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1),
            "Vacation",
            DateTimeOffset.UtcNow);

        timeOff.Cancel(
            DateTimeOffset.UtcNow.AddMinutes(1));

        var result = EmployeeAvailabilityService.Check(
            [CreateWorkingHours(context)],
            [timeOff],
            [],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.Available,
            result);
    }

    [Fact]
    public void CheckWithBookingOverlapShouldReturnBookingConflict()
    {
        var context = CreateContext();

        var booking = CreateBooking(
            context,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        var result = EmployeeAvailabilityService.Check(
            [CreateWorkingHours(context)],
            [],
            [booking],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.BookingConflict,
            result);
    }

    [Fact]
    public void CheckWithExcludedBookingShouldReturnAvailable()
    {
        var context = CreateContext();

        var booking = CreateBooking(
            context,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        var result = EmployeeAvailabilityService.Check(
            [CreateWorkingHours(context)],
            [],
            [booking],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1),
            booking.Id);

        Assert.Equal(
            EmployeeAvailabilityStatus.Available,
            result);
    }

    [Fact]
    public void CheckShouldUseOrganizationTimeZoneForWorkingHours()
    {
        var context = CreateContext();

        var workingHours =
            EmployeeWorkingHours.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.EmployeeId,
                DayOfWeek.Monday,
                new TimeOnly(10, 0),
                new TimeOnly(12, 0));

        var result = EmployeeAvailabilityService.Check(
            [workingHours],
            [],
            [],
            context.OrganizationId,
            context.EmployeeId,
            TestTimeZone,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.Available,
            result);
    }

    [Fact]
    public void CheckCrossingLocalMidnightShouldReturnOutsideWorkingHours()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var startsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                21,
                20,
                30,
                0,
                TimeSpan.Zero);

        var workingHours =
            EmployeeWorkingHours.Create(
                Guid.NewGuid(),
                organizationId,
                employeeId,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(23, 59));

        var result = EmployeeAvailabilityService.Check(
            [workingHours],
            [],
            [],
            organizationId,
            employeeId,
            TestTimeZone,
            startsAtUtc,
            startsAtUtc.AddHours(1));

        Assert.Equal(
            EmployeeAvailabilityStatus.OutsideWorkingHours,
            result);
    }

    [Fact]
    public void CheckWithInvalidTimeRangeShouldThrowArgumentException()
    {
        var context = CreateContext();

        var action = () =>
        {
            EmployeeAvailabilityService.Check(
                [CreateWorkingHours(context)],
                [],
                [],
                context.OrganizationId,
                context.EmployeeId,
                TestTimeZone,
                context.StartsAtUtc,
                context.StartsAtUtc);
        };

        Assert.Throws<ArgumentException>(action);
    }

    private static EmployeeWorkingHours CreateWorkingHours(
        AvailabilityContext context)
    {
        return EmployeeWorkingHours.Create(
            Guid.NewGuid(),
            context.OrganizationId,
            context.EmployeeId,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));
    }

    private static Booking CreateBooking(
        AvailabilityContext context,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        return Booking.Create(
            Guid.NewGuid(),
            context.OrganizationId,
            Guid.NewGuid(),
            context.EmployeeId,
            Guid.NewGuid(),
            startsAtUtc,
            endsAtUtc,
            700m,
            "UAH",
            null,
            DateTimeOffset.UtcNow);
    }

    private static AvailabilityContext CreateContext()
    {
        return new AvailabilityContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(
                2026,
                9,
                21,
                7,
                0,
                0,
                TimeSpan.Zero));
    }

    private sealed record AvailabilityContext(
        Guid OrganizationId,
        Guid EmployeeId,
        DateTimeOffset StartsAtUtc);
}
