using BookingHub.Domain.Customers;
using Xunit;

namespace BookingHub.Domain.UnitTests.Customers;

public sealed class CustomerTagTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateCustomerTag()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var assignedByUserId = Guid.NewGuid();

        var assignedAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var customerTag = CustomerTag.Create(
            Guid.NewGuid(),
            organizationId,
            customerId,
            tagId,
            assignedByUserId,
            assignedAtUtc);

        Assert.Equal(
            organizationId,
            customerTag.OrganizationId);

        Assert.Equal(customerId, customerTag.CustomerId);
        Assert.Equal(tagId, customerTag.TagId);

        Assert.Equal(
            assignedByUserId,
            customerTag.AssignedByUserId);

        Assert.Equal(
            assignedAtUtc,
            customerTag.AssignedAtUtc);
    }

    [Fact]
    public void CreateWithEmptyCustomerIdShouldThrowArgumentException()
    {
        var action = () => CustomerTag.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyTagIdShouldThrowArgumentException()
    {
        var action = () => CustomerTag.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyAssignedByUserIdShouldThrowArgumentException()
    {
        var action = () => CustomerTag.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }
}
