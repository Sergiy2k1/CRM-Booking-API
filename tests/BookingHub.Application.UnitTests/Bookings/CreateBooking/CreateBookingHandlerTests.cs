using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Abstractions.Messaging;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Bookings.IntegrationEvents;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Bookings.CreateBooking;

public sealed class CreateBookingHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository =
        Substitute.For<IOrganizationRepository>();

    private readonly ICustomerRepository _customerRepository =
        Substitute.For<ICustomerRepository>();

    private readonly IEmployeeRepository _employeeRepository =
        Substitute.For<IEmployeeRepository>();

    private readonly IServiceRepository _serviceRepository =
        Substitute.For<IServiceRepository>();

    private readonly IEmployeeServiceRepository _employeeServiceRepository =
        Substitute.For<IEmployeeServiceRepository>();

    private readonly IEmployeeScheduleRepository _employeeScheduleRepository =
        Substitute.For<IEmployeeScheduleRepository>();

    private readonly IBookingRepository _bookingRepository =
        Substitute.For<IBookingRepository>();

    private readonly IUnitOfWork _unitOfWork =
        Substitute.For<IUnitOfWork>();

    private readonly IClock _clock =
        Substitute.For<IClock>();

    private readonly IGuidGenerator _guidGenerator =
        Substitute.For<IGuidGenerator>();

    private readonly IOutboxWriter _outboxWriter =
        Substitute.For<IOutboxWriter>();

    [Fact]
    public async Task HandleWithValidDataShouldCreateBooking()
    {
        var context = CreateContext();
        ConfigureValidScenario(context);

        Booking? addedBooking = null;

        _bookingRepository
            .AddAsync(
                Arg.Do<Booking>(
                    booking => addedBooking = booking),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            CreateCommand(context),
            TestContext.Current.CancellationToken);

        Assert.NotNull(addedBooking);
        Assert.Equal(context.BookingId, addedBooking.Id);
        Assert.Equal(context.OrganizationId, addedBooking.OrganizationId);
        Assert.Equal(context.CustomerId, addedBooking.CustomerId);
        Assert.Equal(context.EmployeeId, addedBooking.EmployeeId);
        Assert.Equal(context.ServiceId, addedBooking.ServiceId);
        Assert.Equal(context.StartsAtUtc, addedBooking.StartsAtUtc);
        Assert.Equal(
            context.StartsAtUtc.AddHours(1),
            addedBooking.EndsAtUtc);
        Assert.Equal(700m, addedBooking.PriceAmount);
        Assert.Equal("UAH", addedBooking.Currency);
        Assert.Equal("First visit", addedBooking.Notes);
        Assert.Equal(BookingStatus.Pending, addedBooking.Status);
        Assert.Equal(context.CreatedAtUtc, addedBooking.CreatedAtUtc);

        Assert.Equal(context.BookingId, result.BookingId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Equal(700m, result.PriceAmount);
        Assert.Equal("UAH", result.Currency);

        await _outboxWriter
            .Received(1)
            .EnqueueAsync(
                BookingEventNames.Created,
                Arg.Is<BookingIntegrationEvent>(
                    integrationEvent =>
                        integrationEvent.BookingId == context.BookingId),
                context.CreatedAtUtc,
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWithBookingConflictShouldThrowBookingUnavailableException()
    {
        var context = CreateContext();
        ConfigureValidScenario(context);

        var conflictingBooking =
            Booking.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                Guid.NewGuid(),
                context.EmployeeId,
                Guid.NewGuid(),
                context.StartsAtUtc.AddMinutes(30),
                context.StartsAtUtc.AddMinutes(90),
                500m,
                "UAH",
                null,
                context.CreatedAtUtc);

        _bookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.StartsAtUtc,
                context.StartsAtUtc.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    [conflictingBooking]));

        var handler = CreateHandler();

        var exception =
            await Assert.ThrowsAsync<BookingUnavailableException>(
                () => handler.HandleAsync(
                    CreateCommand(context),
                    TestContext.Current.CancellationToken));

        Assert.Equal(
            EmployeeAvailabilityStatus.BookingConflict,
            exception.AvailabilityStatus);

        await _bookingRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Booking>(),
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWithCustomerFromDifferentOrganizationShouldThrowEntityNotFoundException()
    {
        var context = CreateContext();
        ConfigureValidScenario(context);

        var foreignCustomer =
            Customer.Create(
                context.CustomerId,
                Guid.NewGuid(),
                "Foreign",
                "Customer",
                null,
                null,
                context.CreatedAtUtc);

        _customerRepository
            .GetByIdAsync(
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Customer?>(
                    foreignCustomer));

        var handler = CreateHandler();

        var exception =
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => handler.HandleAsync(
                    CreateCommand(context),
                    TestContext.Current.CancellationToken));

        Assert.Equal(
            nameof(Customer),
            exception.EntityName);

        Assert.Equal(
            context.CustomerId,
            exception.EntityId);
    }

    [Fact]
    public async Task HandleWithInactiveEmployeeShouldThrowInvalidOperationException()
    {
        var context = CreateContext();
        ConfigureValidScenario(context);

        var inactiveEmployee =
            CreateEmployee(context);

        inactiveEmployee.Deactivate(
            context.CreatedAtUtc.AddMinutes(1));

        _employeeRepository
            .GetByIdAsync(
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    inactiveEmployee));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                CreateCommand(context),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleWithUnassignedServiceShouldThrowInvalidOperationException()
    {
        var context = CreateContext();
        ConfigureValidScenario(context);

        _employeeServiceRepository
            .IsAssignedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                CreateCommand(context),
                TestContext.Current.CancellationToken));

        await _bookingRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Booking>(),
                Arg.Any<CancellationToken>());
    }

    private void ConfigureValidScenario(
        CreateBookingContext context)
    {
        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.CreatedAtUtc);

        var customer =
            Customer.Create(
                context.CustomerId,
                context.OrganizationId,
                "Sergiy",
                "Tester",
                "sergiy@example.com",
                "+380501234567",
                context.CreatedAtUtc);

        var employee =
            CreateEmployee(context);

        var service =
            Service.Create(
                context.ServiceId,
                context.OrganizationId,
                "Haircut",
                "Classic haircut",
                TimeSpan.FromHours(1),
                700m,
                "UAH",
                context.CreatedAtUtc);

        var workingHours =
            EmployeeWorkingHours.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.EmployeeId,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(18, 0));

        _organizationRepository
            .GetByIdAsync(
                context.OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Organization?>(
                    organization));

        _customerRepository
            .GetByIdAsync(
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Customer?>(
                    customer));

        _employeeRepository
            .GetByIdAsync(
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    employee));

        _serviceRepository
            .GetByIdAsync(
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Service?>(
                    service));

        _employeeServiceRepository
            .IsAssignedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        _employeeScheduleRepository
            .GetWorkingHoursAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeWorkingHours>>(
                    [workingHours]));

        _employeeScheduleRepository
            .GetTimeOffAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.StartsAtUtc,
                context.StartsAtUtc.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeTimeOff>>(
                    []));

        _bookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.StartsAtUtc,
                context.StartsAtUtc.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    []));

        _clock.UtcNow.Returns(
            context.CreatedAtUtc);

        _guidGenerator
            .NewGuid()
            .Returns(context.BookingId);

        _unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _bookingRepository
            .AddAsync(
                Arg.Any<Booking>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _outboxWriter
            .EnqueueAsync(
                Arg.Any<string>(),
                Arg.Any<BookingIntegrationEvent>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    private static Employee CreateEmployee(
        CreateBookingContext context)
    {
        return Employee.Create(
            context.EmployeeId,
            context.OrganizationId,
            null,
            "Sergiy",
            "Tester",
            "Barber",
            context.CreatedAtUtc);
    }

    private CreateBookingHandler CreateHandler()
    {
        return new CreateBookingHandler(
            _organizationRepository,
            _customerRepository,
            _employeeRepository,
            _serviceRepository,
            _employeeServiceRepository,
            _employeeScheduleRepository,
            _bookingRepository,
            _unitOfWork,
            _clock,
            _guidGenerator,
            _outboxWriter);
    }

    private static CreateBookingCommand CreateCommand(
        CreateBookingContext context)
    {
        return new CreateBookingCommand(
            context.OrganizationId,
            context.CustomerId,
            context.EmployeeId,
            context.ServiceId,
            context.StartsAtUtc,
            "First visit");
    }

    private static CreateBookingContext CreateContext()
    {
        return new CreateBookingContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(
                2026,
                9,
                21,
                10,
                0,
                0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2026,
                9,
                20,
                12,
                0,
                0,
                TimeSpan.Zero));
    }

    private sealed record CreateBookingContext(
        Guid OrganizationId,
        Guid CustomerId,
        Guid EmployeeId,
        Guid ServiceId,
        Guid BookingId,
        DateTimeOffset StartsAtUtc,
        DateTimeOffset CreatedAtUtc);
}
