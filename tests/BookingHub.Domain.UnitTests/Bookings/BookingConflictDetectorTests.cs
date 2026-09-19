using BookingHub.Domain.Bookings;
using Xunit;

namespace BookingHub.Domain.UnitTests.Bookings;

public sealed class BookingConflictDetectorTests
{
    [Fact]
    public void HasConflictWithOverlappingPendingBookingShouldReturnTrue()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.True(hasConflict);
    }

    [Fact]
    public void HasConflictWithOverlappingConfirmedBookingShouldReturnTrue()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        existingBooking.Confirm(
            DateTimeOffset.UtcNow);

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.True(hasConflict);
    }

    [Fact]
    public void HasConflictWithAdjacentBookingShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddHours(1),
            context.StartsAtUtc.AddHours(2));

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflictForDifferentEmployeeShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            Guid.NewGuid(),
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflictForDifferentOrganizationShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            Guid.NewGuid(),
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflictWithCancelledBookingShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        existingBooking.Cancel(
            DateTimeOffset.UtcNow);

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflictWithCompletedBookingShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        existingBooking.Confirm(
            DateTimeOffset.UtcNow);

        existingBooking.Complete(
            DateTimeOffset.UtcNow.AddMinutes(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflictWithNoShowBookingShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        existingBooking.Confirm(
            DateTimeOffset.UtcNow);

        existingBooking.MarkNoShow(
            DateTimeOffset.UtcNow.AddMinutes(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(30),
            context.StartsAtUtc.AddMinutes(90));

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflictWhenExcludedBookingIsSameShouldReturnFalse()
    {
        var context = CreateContext();
        var existingBooking = CreateBooking(
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc,
            context.StartsAtUtc.AddHours(1));

        var hasConflict = BookingConflictDetector.HasConflict(
            [existingBooking],
            context.OrganizationId,
            context.EmployeeId,
            context.StartsAtUtc.AddMinutes(15),
            context.StartsAtUtc.AddMinutes(45),
            existingBooking.Id);

        Assert.False(hasConflict);
    }

    [Fact]
    public void TimeRangesOverlapWhenOneRangeContainsAnotherShouldReturnTrue()
    {
        var startsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                8,
                0,
                0,
                TimeSpan.Zero);

        var overlaps = BookingConflictDetector.TimeRangesOverlap(
            startsAtUtc,
            startsAtUtc.AddHours(2),
            startsAtUtc.AddMinutes(30),
            startsAtUtc.AddMinutes(60));

        Assert.True(overlaps);
    }

    [Fact]
    public void TimeRangesOverlapWithInvalidRangeShouldThrowArgumentException()
    {
        var startsAtUtc = DateTimeOffset.UtcNow;

        var action = () => BookingConflictDetector.TimeRangesOverlap(
            startsAtUtc,
            startsAtUtc,
            startsAtUtc.AddHours(1),
            startsAtUtc.AddHours(2));

        Assert.Throws<ArgumentException>(action);
    }

    private static Booking CreateBooking(
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        return Booking.Create(
            Guid.NewGuid(),
            organizationId,
            Guid.NewGuid(),
            employeeId,
            Guid.NewGuid(),
            startsAtUtc,
            endsAtUtc,
            700m,
            "UAH",
            null,
            DateTimeOffset.UtcNow);
    }

    private static BookingConflictContext CreateContext()
    {
        return new BookingConflictContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(
                2026,
                9,
                20,
                8,
                0,
                0,
                TimeSpan.Zero));
    }

    private sealed record BookingConflictContext(
        Guid OrganizationId,
        Guid EmployeeId,
        DateTimeOffset StartsAtUtc);
}
