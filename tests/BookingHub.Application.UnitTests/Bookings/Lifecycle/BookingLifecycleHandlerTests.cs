using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Abstractions.Messaging;
using BookingHub.Application.Bookings.ChangeBookingStatus;
using BookingHub.Application.Bookings.GetBooking;
using BookingHub.Application.Bookings.ListBookings;
using BookingHub.Application.Bookings.RescheduleBooking;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Bookings.IntegrationEvents;
using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Bookings.Lifecycle;

public sealed class BookingLifecycleHandlerTests
{
    [Fact]
    public async Task GetShouldReturnBookingFromSameOrganization()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new GetBookingHandler(
                dependencies.BookingRepository);

        var result =
            await handler.HandleAsync(
                new GetBookingQuery(
                    context.OrganizationId,
                    context.BookingId),
                TestContext.Current.CancellationToken);

        Assert.Equal(context.BookingId, result.Id);
        Assert.Equal(context.OrganizationId, result.OrganizationId);
    }

    [Fact]
    public async Task ListShouldReturnRequestedPageAndTotalCount()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.BookingRepository
            .ListAsync(
                context.OrganizationId,
                BookingStatus.Pending,
                context.EmployeeId,
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                20,
                20,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    [context.Booking]));

        dependencies.BookingRepository
            .CountAsync(
                context.OrganizationId,
                BookingStatus.Pending,
                context.EmployeeId,
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(41));

        var handler =
            new ListBookingsHandler(
                dependencies.BookingRepository);

        var result =
            await handler.HandleAsync(
                new ListBookingsQuery(
                    context.OrganizationId,
                    BookingStatus.Pending,
                    context.EmployeeId,
                    null,
                    null,
                    2,
                    20),
                TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(41, result.TotalCount);
    }

    [Fact]
    public async Task ConfirmPendingBookingShouldChangeStatus()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new ChangeBookingStatusHandler(
                dependencies.BookingRepository,
                dependencies.Clock,
                dependencies.UnitOfWork,
                dependencies.OutboxWriter);

        var result =
            await handler.HandleAsync(
                new ChangeBookingStatusCommand(
                    context.OrganizationId,
                    context.BookingId,
                    BookingTransition.Confirm),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            BookingStatus.Confirmed,
            result.Status);
    }

    [Fact]
    public async Task RescheduleAvailableBookingShouldPreserveDuration()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var newStart =
            context.Booking.StartsAtUtc.AddDays(7);

        dependencies.EmployeeScheduleRepository
            .GetWorkingHoursAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeWorkingHours>>(
                    [
                        EmployeeWorkingHours.Create(
                            Guid.NewGuid(),
                            context.OrganizationId,
                            context.EmployeeId,
                            newStart.DayOfWeek,
                            new TimeOnly(9, 0),
                            new TimeOnly(18, 0))
                    ]));

        dependencies.BookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                newStart,
                newStart.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    [context.Booking]));

        var handler =
            new RescheduleBookingHandler(
                dependencies.BookingRepository,
                dependencies.OrganizationRepository,
                dependencies.EmployeeRepository,
                dependencies.EmployeeScheduleRepository,
                dependencies.Clock,
                dependencies.UnitOfWork,
                dependencies.OutboxWriter);

        var result =
            await handler.HandleAsync(
                new RescheduleBookingCommand(
                    context.OrganizationId,
                    context.BookingId,
                    newStart),
                TestContext.Current.CancellationToken);

        Assert.Equal(newStart, result.StartsAtUtc);
        Assert.Equal(newStart.AddHours(1), result.EndsAtUtc);
    }

    [Fact]
    public async Task RescheduleWithConflictShouldThrowBookingUnavailableException()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var newStart =
            context.Booking.StartsAtUtc.AddDays(7);

        dependencies.EmployeeScheduleRepository
            .GetWorkingHoursAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeWorkingHours>>(
                    [
                        EmployeeWorkingHours.Create(
                            Guid.NewGuid(),
                            context.OrganizationId,
                            context.EmployeeId,
                            newStart.DayOfWeek,
                            new TimeOnly(9, 0),
                            new TimeOnly(18, 0))
                    ]));

        var otherBooking =
            Booking.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                Guid.NewGuid(),
                context.EmployeeId,
                Guid.NewGuid(),
                newStart.AddMinutes(30),
                newStart.AddMinutes(90),
                500m,
                "UAH",
                null,
                context.UtcNow);

        dependencies.BookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                newStart,
                newStart.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    [otherBooking]));

        var handler =
            new RescheduleBookingHandler(
                dependencies.BookingRepository,
                dependencies.OrganizationRepository,
                dependencies.EmployeeRepository,
                dependencies.EmployeeScheduleRepository,
                dependencies.Clock,
                dependencies.UnitOfWork,
                dependencies.OutboxWriter);

        var exception =
            await Assert.ThrowsAsync<BookingUnavailableException>(
                () => handler.HandleAsync(
                    new RescheduleBookingCommand(
                        context.OrganizationId,
                        context.BookingId,
                        newStart),
                    TestContext.Current.CancellationToken));

        Assert.Equal(
            EmployeeAvailabilityStatus.BookingConflict,
            exception.AvailabilityStatus);
    }

    private static TestDependencies ConfigureDependencies(
        BookingTestContext context)
    {
        var bookingRepository =
            Substitute.For<IBookingRepository>();

        var organizationRepository =
            Substitute.For<IOrganizationRepository>();

        var employeeRepository =
            Substitute.For<IEmployeeRepository>();

        var employeeScheduleRepository =
            Substitute.For<IEmployeeScheduleRepository>();

        var clock =
            Substitute.For<IClock>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        var outboxWriter =
            Substitute.For<IOutboxWriter>();

        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.UtcNow);

        var employee =
            Employee.Create(
                context.EmployeeId,
                context.OrganizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                context.UtcNow.AddDays(-1));

        bookingRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.BookingId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Booking?>(
                    context.Booking));

        bookingRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.BookingId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Booking?>(
                    context.Booking));

        organizationRepository
            .GetByIdAsync(
                context.OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Organization?>(
                    organization));

        employeeRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    employee));

        employeeScheduleRepository
            .GetTimeOffAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeTimeOff>>(
                    []));

        clock.UtcNow.Returns(
            context.UtcNow);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        outboxWriter
            .EnqueueAsync(
                Arg.Any<string>(),
                Arg.Any<BookingIntegrationEvent>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return new TestDependencies(
            bookingRepository,
            organizationRepository,
            employeeRepository,
            employeeScheduleRepository,
            clock,
            unitOfWork,
            outboxWriter);
    }

    private static BookingTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                20,
                0,
                0,
                TimeSpan.Zero);

        var startsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                21,
                10,
                0,
                0,
                TimeSpan.Zero);

        var booking =
            Booking.Create(
                bookingId,
                organizationId,
                Guid.NewGuid(),
                employeeId,
                Guid.NewGuid(),
                startsAtUtc,
                startsAtUtc.AddHours(1),
                700m,
                "UAH",
                "First visit",
                utcNow);

        return new BookingTestContext(
            organizationId,
            employeeId,
            bookingId,
            utcNow,
            booking);
    }

    private sealed record BookingTestContext(
        Guid OrganizationId,
        Guid EmployeeId,
        Guid BookingId,
        DateTimeOffset UtcNow,
        Booking Booking);

    private sealed record TestDependencies(
        IBookingRepository BookingRepository,
        IOrganizationRepository OrganizationRepository,
        IEmployeeRepository EmployeeRepository,
        IEmployeeScheduleRepository EmployeeScheduleRepository,
        IClock Clock,
        IUnitOfWork UnitOfWork,
        IOutboxWriter OutboxWriter);
}
