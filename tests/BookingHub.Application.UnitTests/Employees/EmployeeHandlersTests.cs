using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.CreateEmployee;
using BookingHub.Application.Employees.GetEmployee;
using BookingHub.Application.Employees.ListEmployees;
using BookingHub.Application.Employees.Schedules.CancelTimeOff;
using BookingHub.Application.Employees.Schedules.CreateTimeOff;
using BookingHub.Application.Employees.Schedules.CreateWorkingHours;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Employees;

public sealed class EmployeeHandlersTests
{
    [Fact]
    public async Task CreateWithValidDataShouldPersistEmployee()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new CreateEmployeeHandler(
                dependencies.OrganizationRepository,
                dependencies.EmployeeRepository,
                dependencies.GuidGenerator,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new CreateEmployeeCommand(
                    context.OrganizationId,
                    " Sergiy ",
                    " Tester ",
                    " Barber "),
                TestContext.Current.CancellationToken);

        Assert.Equal(context.EmployeeId, result.Id);
        Assert.Equal("Sergiy", result.FirstName);
        Assert.Equal("Tester", result.LastName);
        Assert.Equal("Barber", result.Position);

        await dependencies.EmployeeRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Employee>(
                    employee =>
                        employee.Id == context.EmployeeId &&
                        employee.OrganizationId == context.OrganizationId),
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetFromDifferentOrganizationShouldThrowEntityNotFoundException()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.EmployeeRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Employee?>(null));

        var handler =
            new GetEmployeeHandler(
                dependencies.EmployeeRepository);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.HandleAsync(
                new GetEmployeeQuery(
                    context.OrganizationId,
                    context.EmployeeId),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListShouldReturnRequestedPageAndTotalCount()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.EmployeeRepository
            .ListAsync(
                context.OrganizationId,
                EmployeeStatus.Active,
                20,
                20,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Employee>>(
                    [context.Employee]));

        dependencies.EmployeeRepository
            .CountAsync(
                context.OrganizationId,
                EmployeeStatus.Active,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(41));

        var handler =
            new ListEmployeesHandler(
                dependencies.EmployeeRepository);

        var result =
            await handler.HandleAsync(
                new ListEmployeesQuery(
                    context.OrganizationId,
                    EmployeeStatus.Active,
                    2,
                    20),
                TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(41, result.TotalCount);
    }

    [Fact]
    public async Task CreateOverlappingWorkingHoursShouldThrowInvalidOperationException()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.EmployeeScheduleRepository
            .HasWorkingHoursOverlapAsync(
                context.OrganizationId,
                context.EmployeeId,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(17, 0),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var handler =
            new CreateWorkingHoursHandler(
                dependencies.EmployeeRepository,
                dependencies.EmployeeScheduleRepository,
                dependencies.GuidGenerator,
                dependencies.UnitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                new CreateWorkingHoursCommand(
                    context.OrganizationId,
                    context.EmployeeId,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(17, 0)),
                TestContext.Current.CancellationToken));

        await dependencies.EmployeeScheduleRepository
            .DidNotReceive()
            .AddWorkingHoursAsync(
                Arg.Any<EmployeeWorkingHours>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTimeOffShouldPersistActiveTimeOff()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.GuidGenerator.NewGuid()
            .Returns(context.TimeOffId);

        var handler =
            new CreateTimeOffHandler(
                dependencies.EmployeeRepository,
                dependencies.EmployeeScheduleRepository,
                dependencies.GuidGenerator,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var startsAtUtc =
            context.UtcNow.AddDays(1);

        var endsAtUtc =
            startsAtUtc.AddHours(8);

        var result =
            await handler.HandleAsync(
                new CreateTimeOffCommand(
                    context.OrganizationId,
                    context.EmployeeId,
                    startsAtUtc,
                    endsAtUtc,
                    "Vacation"),
                TestContext.Current.CancellationToken);

        Assert.Equal(context.TimeOffId, result.Id);
        Assert.Equal(EmployeeTimeOffStatus.Active, result.Status);
        Assert.Equal("Vacation", result.Reason);

        await dependencies.EmployeeScheduleRepository
            .Received(1)
            .AddTimeOffAsync(
                Arg.Is<EmployeeTimeOff>(
                    timeOff =>
                        timeOff.Id == context.TimeOffId &&
                        timeOff.EmployeeId == context.EmployeeId),
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CancelTimeOffShouldChangeStatusToCancelled()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var timeOff =
            EmployeeTimeOff.Create(
                context.TimeOffId,
                context.OrganizationId,
                context.EmployeeId,
                context.UtcNow.AddDays(1),
                context.UtcNow.AddDays(1).AddHours(8),
                "Vacation",
                context.UtcNow.AddDays(-1));

        dependencies.EmployeeScheduleRepository
            .GetTrackedTimeOffByIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.TimeOffId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<EmployeeTimeOff?>(
                    timeOff));

        var handler =
            new CancelTimeOffHandler(
                dependencies.EmployeeScheduleRepository,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new CancelTimeOffCommand(
                    context.OrganizationId,
                    context.EmployeeId,
                    context.TimeOffId),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            EmployeeTimeOffStatus.Cancelled,
            result.Status);

        Assert.Equal(
            context.UtcNow,
            result.CancelledAtUtc);
    }

    private static TestDependencies ConfigureDependencies(
        EmployeeTestContext context)
    {
        var organizationRepository =
            Substitute.For<IOrganizationRepository>();

        var employeeRepository =
            Substitute.For<IEmployeeRepository>();

        var employeeScheduleRepository =
            Substitute.For<IEmployeeScheduleRepository>();

        var guidGenerator =
            Substitute.For<IGuidGenerator>();

        var clock =
            Substitute.For<IClock>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.UtcNow);

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
                    context.Employee));

        employeeRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    context.Employee));

        employeeRepository
            .AddAsync(
                Arg.Any<Employee>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeScheduleRepository
            .AddWorkingHoursAsync(
                Arg.Any<EmployeeWorkingHours>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeScheduleRepository
            .AddTimeOffAsync(
                Arg.Any<EmployeeTimeOff>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeScheduleRepository
            .HasWorkingHoursOverlapAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DayOfWeek>(),
                Arg.Any<TimeOnly>(),
                Arg.Any<TimeOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        guidGenerator.NewGuid()
            .Returns(context.EmployeeId);

        clock.UtcNow.Returns(
            context.UtcNow);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return new TestDependencies(
            organizationRepository,
            employeeRepository,
            employeeScheduleRepository,
            guidGenerator,
            clock,
            unitOfWork);
    }

    private static EmployeeTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var timeOffId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                18,
                0,
                0,
                TimeSpan.Zero);

        var employee =
            Employee.Create(
                employeeId,
                organizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                utcNow.AddDays(-1));

        return new EmployeeTestContext(
            organizationId,
            employeeId,
            timeOffId,
            utcNow,
            employee);
    }

    private sealed record EmployeeTestContext(
        Guid OrganizationId,
        Guid EmployeeId,
        Guid TimeOffId,
        DateTimeOffset UtcNow,
        Employee Employee);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        IEmployeeRepository EmployeeRepository,
        IEmployeeScheduleRepository EmployeeScheduleRepository,
        IGuidGenerator GuidGenerator,
        IClock Clock,
        IUnitOfWork UnitOfWork);
}
