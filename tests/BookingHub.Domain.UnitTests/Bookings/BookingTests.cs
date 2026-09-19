using BookingHub.Domain.Bookings;
using Xunit;

namespace BookingHub.Domain.UnitTests.Bookings;

public sealed class BookingTests
{
    [Fact]
    public void CreateWithValidDataShouldCreatePendingBooking()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

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
            startsAtUtc.AddMinutes(45);

        var createdAtUtc =
            startsAtUtc.AddDays(-1);

        var booking = Booking.Create(
            Guid.NewGuid(),
            organizationId,
            customerId,
            employeeId,
            serviceId,
            startsAtUtc,
            endsAtUtc,
            700m,
            "uah",
            "First visit",
            createdAtUtc);

        Assert.Equal(organizationId, booking.OrganizationId);
        Assert.Equal(customerId, booking.CustomerId);
        Assert.Equal(employeeId, booking.EmployeeId);
        Assert.Equal(serviceId, booking.ServiceId);
        Assert.Equal(startsAtUtc, booking.StartsAtUtc);
        Assert.Equal(endsAtUtc, booking.EndsAtUtc);
        Assert.Equal(700m, booking.PriceAmount);
        Assert.Equal("UAH", booking.Currency);
        Assert.Equal("First visit", booking.Notes);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(createdAtUtc, booking.CreatedAtUtc);
        Assert.Equal(createdAtUtc, booking.UpdatedAtUtc);
        Assert.Null(booking.CancelledAtUtc);
        Assert.Null(booking.CompletedAtUtc);
        Assert.Null(booking.NoShowAtUtc);
    }

    [Fact]
    public void CreateWithInvalidTimeRangeShouldThrowArgumentException()
    {
        var startsAtUtc = DateTimeOffset.UtcNow;

        var action = () => Booking.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc,
            700m,
            "UAH",
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithNegativePriceShouldThrowArgumentOutOfRangeException()
    {
        var startsAtUtc = DateTimeOffset.UtcNow;

        var action = () => Booking.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc.AddMinutes(45),
            -1m,
            "UAH",
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void CreateWithInvalidCurrencyShouldThrowArgumentException()
    {
        var startsAtUtc = DateTimeOffset.UtcNow;

        var action = () => Booking.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc.AddMinutes(45),
            700m,
            "UA",
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ConfirmPendingBookingShouldChangeStatusToConfirmed()
    {
        var booking = CreateBooking();
        var confirmedAtUtc = DateTimeOffset.UtcNow;

        booking.Confirm(confirmedAtUtc);

        Assert.Equal(
            BookingStatus.Confirmed,
            booking.Status);

        Assert.Equal(
            confirmedAtUtc.ToUniversalTime(),
            booking.UpdatedAtUtc);
    }

    [Fact]
    public void ConfirmConfirmedBookingShouldThrowInvalidOperationException()
    {
        var booking = CreateBooking();

        booking.Confirm(
            DateTimeOffset.UtcNow);

        var action = () => booking.Confirm(
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            action);
    }

    [Fact]
    public void RescheduleConfirmedBookingShouldChangeTimeRange()
    {
        var booking = CreateConfirmedBooking();

        var newStart =
            DateTimeOffset.UtcNow.AddDays(2);

        var newEnd =
            newStart.AddHours(1);

        var updatedAtUtc =
            DateTimeOffset.UtcNow;

        booking.Reschedule(
            newStart,
            newEnd,
            updatedAtUtc);

        Assert.Equal(
            newStart.ToUniversalTime(),
            booking.StartsAtUtc);

        Assert.Equal(
            newEnd.ToUniversalTime(),
            booking.EndsAtUtc);

        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            booking.UpdatedAtUtc);
    }

    [Fact]
    public void CancelPendingBookingShouldChangeStatusToCancelled()
    {
        var booking = CreateBooking();
        var cancelledAtUtc = DateTimeOffset.UtcNow;

        booking.Cancel(cancelledAtUtc);

        Assert.Equal(
            BookingStatus.Cancelled,
            booking.Status);

        Assert.Equal(
            cancelledAtUtc.ToUniversalTime(),
            booking.CancelledAtUtc);
    }

    [Fact]
    public void CompleteConfirmedBookingShouldChangeStatusToCompleted()
    {
        var booking = CreateConfirmedBooking();
        var completedAtUtc = DateTimeOffset.UtcNow;

        booking.Complete(completedAtUtc);

        Assert.Equal(
            BookingStatus.Completed,
            booking.Status);

        Assert.Equal(
            completedAtUtc.ToUniversalTime(),
            booking.CompletedAtUtc);
    }

    [Fact]
    public void CompletePendingBookingShouldThrowInvalidOperationException()
    {
        var booking = CreateBooking();

        var action = () => booking.Complete(
            DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(
            action);
    }

    [Fact]
    public void MarkNoShowForConfirmedBookingShouldChangeStatusToNoShow()
    {
        var booking = CreateConfirmedBooking();
        var noShowAtUtc = DateTimeOffset.UtcNow;

        booking.MarkNoShow(noShowAtUtc);

        Assert.Equal(
            BookingStatus.NoShow,
            booking.Status);

        Assert.Equal(
            noShowAtUtc.ToUniversalTime(),
            booking.NoShowAtUtc);
    }

    [Fact]
    public void RescheduleCancelledBookingShouldThrowInvalidOperationException()
    {
        var booking = CreateBooking();

        booking.Cancel(
            DateTimeOffset.UtcNow);

        var newStart =
            DateTimeOffset.UtcNow.AddDays(2);

        var action = () => booking.Reschedule(
            newStart,
            newStart.AddHours(1),
            DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(
            action);
    }

    [Fact]
    public void UpdateNotesShouldNormalizeWhitespaceOnlyValueToNull()
    {
        var booking = CreateBooking();

        booking.UpdateNotes(
            "   ",
            DateTimeOffset.UtcNow);

        Assert.Null(booking.Notes);
    }

    private static Booking CreateBooking()
    {
        var startsAtUtc =
            DateTimeOffset.UtcNow.AddDays(1);

        return Booking.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAtUtc,
            startsAtUtc.AddMinutes(45),
            700m,
            "UAH",
            null,
            DateTimeOffset.UtcNow);
    }

    private static Booking CreateConfirmedBooking()
    {
        var booking = CreateBooking();

        booking.Confirm(
            DateTimeOffset.UtcNow);

        return booking;
    }
}
