using BookingHub.Domain.Employees;
using Xunit;

namespace BookingHub.Domain.UnitTests.Employees;

public sealed class EmployeeTimeOffTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveTimeOff()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var startsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                8,
                0,
                0,
                TimeSpan.Zero);

        var endsAtUtc =
            startsAtUtc.AddHours(4);

        var createdAtUtc =
            startsAtUtc.AddDays(-1);

        var timeOff = EmployeeTimeOff.Create(
            Guid.NewGuid(),
            organizationId,
            employeeId,
            startsAtUtc,
            endsAtUtc,
            "Medical appointment",
            createdAtUtc);

        Assert.Equal(
            organizationId,
            timeOff.OrganizationId);

        Assert.Equal(
            employeeId,
            timeOff.EmployeeId);

        Assert.Equal(startsAtUtc, timeOff.StartsAtUtc);
        Assert.Equal(endsAtUtc, timeOff.EndsAtUtc);
        Assert.Equal("Medical appointment", timeOff.Reason);

        Assert.Equal(
            EmployeeTimeOffStatus.Active,
            timeOff.Status);

        Assert.Null(timeOff.CancelledAtUtc);
    }

    [Fact]
    public void CreateWithInvalidTimeRangeShouldThrowArgumentException()
    {
        var startsAtUtc = DateTimeOffset.UtcNow;

        var action = () => EmployeeTimeOff.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc,
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithWhitespaceReasonShouldStoreNull()
    {
        var startsAtUtc = DateTimeOffset.UtcNow;

        var timeOff = EmployeeTimeOff.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc.AddHours(1),
            "   ",
            DateTimeOffset.UtcNow);

        Assert.Null(timeOff.Reason);
    }

    [Fact]
    public void RescheduleShouldChangeTimeRange()
    {
        var timeOff = CreateTimeOff();
        var newStart = DateTimeOffset.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(2);

        timeOff.Reschedule(
            newStart,
            newEnd,
            DateTimeOffset.UtcNow);

        Assert.Equal(
            newStart.ToUniversalTime(),
            timeOff.StartsAtUtc);

        Assert.Equal(
            newEnd.ToUniversalTime(),
            timeOff.EndsAtUtc);
    }

    [Fact]
    public void CancelShouldMarkTimeOffAsCancelled()
    {
        var timeOff = CreateTimeOff();
        var cancelledAtUtc = DateTimeOffset.UtcNow;

        timeOff.Cancel(cancelledAtUtc);

        Assert.Equal(
            EmployeeTimeOffStatus.Cancelled,
            timeOff.Status);

        Assert.Equal(
            cancelledAtUtc.ToUniversalTime(),
            timeOff.CancelledAtUtc);
    }

    [Fact]
    public void UpdateCancelledTimeOffShouldThrowInvalidOperationException()
    {
        var timeOff = CreateTimeOff();

        timeOff.Cancel(
            DateTimeOffset.UtcNow);

        var action = () => timeOff.UpdateReason(
            "Changed reason",
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Throws<InvalidOperationException>(
            action);
    }

    private static EmployeeTimeOff CreateTimeOff()
    {
        var startsAtUtc =
            DateTimeOffset.UtcNow.AddDays(1);

        return EmployeeTimeOff.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc.AddHours(2),
            "Vacation",
            DateTimeOffset.UtcNow);
    }
}
