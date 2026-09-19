using BookingHub.Domain.Customers;
using Xunit;

namespace BookingHub.Domain.UnitTests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveCustomer()
    {
        var id = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var customer = Customer.Create(
            id,
            organizationId,
            "Sergiy",
            "Tester",
            "sergiy@example.com",
            "+380501234567",
            createdAtUtc);

        Assert.Equal(id, customer.Id);
        Assert.Equal(organizationId, customer.OrganizationId);
        Assert.Equal("Sergiy", customer.FirstName);
        Assert.Equal("Tester", customer.LastName);
        Assert.Equal("sergiy@example.com", customer.Email);
        Assert.Equal("+380501234567", customer.Phone);
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.Equal(createdAtUtc, customer.CreatedAtUtc);
        Assert.Equal(createdAtUtc, customer.UpdatedAtUtc);
        Assert.Null(customer.ArchivedAtUtc);
    }

    [Fact]
    public void CreateWithEmptyOrganizationIdShouldThrowArgumentException()
    {
        var action = () => Customer.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Sergiy",
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyFirstNameShouldThrowArgumentException()
    {
        var action = () => Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithWhitespaceOptionalValuesShouldStoreNull()
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sergiy",
            "   ",
            "   ",
            "   ",
            DateTimeOffset.UtcNow);

        Assert.Null(customer.LastName);
        Assert.Null(customer.Email);
        Assert.Null(customer.Phone);
    }

    [Fact]
    public void UpdateNameShouldChangeCustomerName()
    {
        var customer = CreateCustomer();
        var updatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(10);

        customer.UpdateName(
            "New",
            "Name",
            updatedAtUtc);

        Assert.Equal("New", customer.FirstName);
        Assert.Equal("Name", customer.LastName);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            customer.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateContactDetailsShouldChangeContacts()
    {
        var customer = CreateCustomer();
        var updatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(10);

        customer.UpdateContactDetails(
            "new@example.com",
            "+380671234567",
            updatedAtUtc);

        Assert.Equal("new@example.com", customer.Email);
        Assert.Equal("+380671234567", customer.Phone);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            customer.UpdatedAtUtc);
    }

    [Fact]
    public void ArchiveShouldMarkCustomerAsArchived()
    {
        var customer = CreateCustomer();
        var archivedAtUtc = DateTimeOffset.UtcNow.AddMinutes(10);

        customer.Archive(archivedAtUtc);

        Assert.Equal(
            CustomerStatus.Archived,
            customer.Status);

        Assert.Equal(
            archivedAtUtc.ToUniversalTime(),
            customer.ArchivedAtUtc);

        Assert.Equal(
            archivedAtUtc.ToUniversalTime(),
            customer.UpdatedAtUtc);
    }

    [Fact]
    public void RestoreShouldReactivateArchivedCustomer()
    {
        var customer = CreateCustomer();

        customer.Archive(
            DateTimeOffset.UtcNow);

        customer.Restore(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Equal(
            CustomerStatus.Active,
            customer.Status);

        Assert.Null(customer.ArchivedAtUtc);
    }

    [Fact]
    public void UpdateArchivedCustomerShouldThrowInvalidOperationException()
    {
        var customer = CreateCustomer();

        customer.Archive(
            DateTimeOffset.UtcNow);

        var action = () => customer.UpdateName(
            "New",
            "Name",
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Throws<InvalidOperationException>(
            action);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sergiy",
            "Tester",
            "sergiy@example.com",
            "+380501234567",
            DateTimeOffset.UtcNow);
    }
}
