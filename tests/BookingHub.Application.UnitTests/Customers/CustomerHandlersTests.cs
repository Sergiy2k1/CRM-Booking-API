using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Customers.ArchiveCustomer;
using BookingHub.Application.Customers.CreateCustomer;
using BookingHub.Application.Customers.GetCustomer;
using BookingHub.Application.Customers.ListCustomers;
using BookingHub.Application.Customers.RestoreCustomer;
using BookingHub.Application.Customers.UpdateCustomer;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Customers;

public sealed class CustomerHandlersTests
{
    [Fact]
    public async Task CreateWithValidDataShouldPersistCustomer()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new CreateCustomerHandler(
                dependencies.OrganizationRepository,
                dependencies.CustomerRepository,
                dependencies.GuidGenerator,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new CreateCustomerCommand(
                    context.OrganizationId,
                    " Sergiy ",
                    " Tester ",
                    "sergiy@example.com",
                    "+380501234567"),
                TestContext.Current.CancellationToken);

        Assert.Equal(context.CustomerId, result.Id);
        Assert.Equal("Sergiy", result.FirstName);
        Assert.Equal("Tester", result.LastName);

        await dependencies.CustomerRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Customer>(
                    customer =>
                        customer.Id == context.CustomerId &&
                        customer.OrganizationId == context.OrganizationId),
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetFromDifferentOrganizationShouldThrowEntityNotFoundException()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.CustomerRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Customer?>(null));

        var handler =
            new GetCustomerHandler(
                dependencies.CustomerRepository);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.HandleAsync(
                new GetCustomerQuery(
                    context.OrganizationId,
                    context.CustomerId),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListShouldReturnRequestedPageAndTotalCount()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.CustomerRepository
            .ListAsync(
                context.OrganizationId,
                CustomerStatus.Active,
                20,
                20,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Customer>>(
                    [context.Customer]));

        dependencies.CustomerRepository
            .CountAsync(
                context.OrganizationId,
                CustomerStatus.Active,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(41));

        var handler =
            new ListCustomersHandler(
                dependencies.CustomerRepository);

        var result =
            await handler.HandleAsync(
                new ListCustomersQuery(
                    context.OrganizationId,
                    CustomerStatus.Active,
                    2,
                    20),
                TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(41, result.TotalCount);
    }

    [Fact]
    public async Task UpdateShouldChangeCustomerData()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new UpdateCustomerHandler(
                dependencies.CustomerRepository,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new UpdateCustomerCommand(
                    context.OrganizationId,
                    context.CustomerId,
                    "Updated",
                    "Customer",
                    "updated@example.com",
                    "+380671234567"),
                TestContext.Current.CancellationToken);

        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Customer", result.LastName);
        Assert.Equal("updated@example.com", result.Email);
    }

    [Fact]
    public async Task ArchiveShouldChangeStatusToArchived()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var handler =
            new ArchiveCustomerHandler(
                dependencies.CustomerRepository,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new ArchiveCustomerCommand(
                    context.OrganizationId,
                    context.CustomerId),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            CustomerStatus.Archived,
            result.Status);
    }

    [Fact]
    public async Task RestoreShouldChangeStatusToActive()
    {
        var context = CreateContext();

        context.Customer.Archive(
            context.UtcNow.AddMinutes(-1));

        var dependencies = ConfigureDependencies(context);

        var handler =
            new RestoreCustomerHandler(
                dependencies.CustomerRepository,
                dependencies.Clock,
                dependencies.UnitOfWork);

        var result =
            await handler.HandleAsync(
                new RestoreCustomerCommand(
                    context.OrganizationId,
                    context.CustomerId),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            CustomerStatus.Active,
            result.Status);
    }

    private static TestDependencies ConfigureDependencies(
        CustomerTestContext context)
    {
        var organizationRepository =
            Substitute.For<IOrganizationRepository>();

        var customerRepository =
            Substitute.For<ICustomerRepository>();

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

        customerRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Customer?>(
                    context.Customer));

        customerRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Customer?>(
                    context.Customer));

        customerRepository
            .AddAsync(
                Arg.Any<Customer>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        guidGenerator.NewGuid()
            .Returns(context.CustomerId);

        clock.UtcNow.Returns(
            context.UtcNow);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return new TestDependencies(
            organizationRepository,
            customerRepository,
            guidGenerator,
            clock,
            unitOfWork);
    }

    private static CustomerTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                17,
                0,
                0,
                TimeSpan.Zero);

        var customer =
            Customer.Create(
                customerId,
                organizationId,
                "Sergiy",
                "Tester",
                "sergiy@example.com",
                "+380501234567",
                utcNow.AddDays(-1));

        return new CustomerTestContext(
            organizationId,
            customerId,
            utcNow,
            customer);
    }

    private sealed record CustomerTestContext(
        Guid OrganizationId,
        Guid CustomerId,
        DateTimeOffset UtcNow,
        Customer Customer);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        ICustomerRepository CustomerRepository,
        IGuidGenerator GuidGenerator,
        IClock Clock,
        IUnitOfWork UnitOfWork);
}
