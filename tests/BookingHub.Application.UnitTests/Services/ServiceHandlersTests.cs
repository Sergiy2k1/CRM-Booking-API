using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Services.Assignments.AssignService;
using BookingHub.Application.Services.Assignments.UnassignService;
using BookingHub.Application.Services.CreateService;
using BookingHub.Application.Services.ListServices;
using BookingHub.Application.Services.UpdateService;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Services;

public sealed class ServiceHandlersTests
{
    [Fact]
    public async Task CreateWithValidDataShouldPersistService()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new CreateServiceHandler(
                dependencies.OrganizationRepository,
                dependencies.ServiceRepository,
                dependencies.GuidGenerator,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new CreateServiceCommand(
                    context.OrganizationId,
                    " Haircut ",
                    " Classic ",
                    TimeSpan.FromHours(1),
                    700m,
                    "uah"),
                TestContext.Current.CancellationToken);

        Assert.Equal(context.ServiceId, result.Id);
        Assert.Equal("Haircut", result.Name);
        Assert.Equal("UAH", result.Currency);

        await dependencies.ServiceRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Service>(
                    service =>
                        service.Id == context.ServiceId &&
                        service.OrganizationId == context.OrganizationId),
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ListShouldReturnRequestedPageAndTotalCount()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.ServiceRepository
            .ListAsync(
                context.OrganizationId,
                ServiceStatus.Active,
                20,
                20,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Service>>(
                    [context.Service]));

        dependencies.ServiceRepository
            .CountAsync(
                context.OrganizationId,
                ServiceStatus.Active,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(41));

        var handler =
            new ListServicesHandler(
                dependencies.ServiceRepository);

        var result =
            await handler.HandleAsync(
                new ListServicesQuery(
                    context.OrganizationId,
                    ServiceStatus.Active,
                    2,
                    20),
                TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(41, result.TotalCount);
    }

    [Fact]
    public async Task UpdateShouldChangeServiceDetails()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new UpdateServiceHandler(
                dependencies.ServiceRepository,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new UpdateServiceCommand(
                    context.OrganizationId,
                    context.ServiceId,
                    "Premium Haircut",
                    "Updated",
                    TimeSpan.FromMinutes(90),
                    900m,
                    "USD"),
                TestContext.Current.CancellationToken);

        Assert.Equal("Premium Haircut", result.Name);
        Assert.Equal(900m, result.PriceAmount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task AssignDuplicateServiceShouldThrowInvalidOperationException()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.EmployeeServiceRepository
            .IsAssignedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var handler =
            new AssignServiceHandler(
                dependencies.EmployeeRepository,
                dependencies.ServiceRepository,
                dependencies.EmployeeServiceRepository,
                dependencies.GuidGenerator,
                dependencies.Clock,
                dependencies.UnitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                new AssignServiceCommand(
                    context.OrganizationId,
                    context.EmployeeId,
                    context.ServiceId),
                TestContext.Current.CancellationToken));

        await dependencies.EmployeeServiceRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<EmployeeService>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnassignShouldRemoveAssignment()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var assignment =
            EmployeeService.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                context.UtcNow.AddDays(-1));

        dependencies.EmployeeServiceRepository
            .GetTrackedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<EmployeeService?>(
                    assignment));

        var handler =
            new UnassignServiceHandler(
                dependencies.EmployeeServiceRepository,
                dependencies.UnitOfWork);

        await handler.HandleAsync(
            new UnassignServiceCommand(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId),
            TestContext.Current.CancellationToken);

        dependencies.EmployeeServiceRepository
            .Received(1)
            .Remove(assignment);
    }

    private static TestDependencies ConfigureDependencies(
        ServiceTestContext context)
    {
        var organizationRepository =
            Substitute.For<IOrganizationRepository>();

        var serviceRepository =
            Substitute.For<IServiceRepository>();

        var employeeRepository =
            Substitute.For<IEmployeeRepository>();

        var employeeServiceRepository =
            Substitute.For<IEmployeeServiceRepository>();

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

        serviceRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Service?>(
                    context.Service));

        serviceRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Service?>(
                    context.Service));

        serviceRepository
            .AddAsync(
                Arg.Any<Service>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    context.Employee));

        employeeServiceRepository
            .IsAssignedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        employeeServiceRepository
            .AddAsync(
                Arg.Any<EmployeeService>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        guidGenerator.NewGuid()
            .Returns(context.ServiceId);

        clock.UtcNow.Returns(context.UtcNow);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return new TestDependencies(
            organizationRepository,
            serviceRepository,
            employeeRepository,
            employeeServiceRepository,
            guidGenerator,
            clock,
            unitOfWork);
    }

    private static ServiceTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                19,
                0,
                0,
                TimeSpan.Zero);

        var service =
            Service.Create(
                serviceId,
                organizationId,
                "Haircut",
                "Classic",
                TimeSpan.FromHours(1),
                700m,
                "UAH",
                utcNow.AddDays(-1));

        var employee =
            Employee.Create(
                employeeId,
                organizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                utcNow.AddDays(-1));

        return new ServiceTestContext(
            organizationId,
            serviceId,
            employeeId,
            utcNow,
            service,
            employee);
    }

    private sealed record ServiceTestContext(
        Guid OrganizationId,
        Guid ServiceId,
        Guid EmployeeId,
        DateTimeOffset UtcNow,
        Service Service,
        Employee Employee);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        IServiceRepository ServiceRepository,
        IEmployeeRepository EmployeeRepository,
        IEmployeeServiceRepository EmployeeServiceRepository,
        IGuidGenerator GuidGenerator,
        IClock Clock,
        IUnitOfWork UnitOfWork);
}
