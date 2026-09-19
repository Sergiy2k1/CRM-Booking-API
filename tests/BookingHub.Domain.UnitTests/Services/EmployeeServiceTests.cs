using BookingHub.Domain.Services;
using Xunit;

namespace BookingHub.Domain.UnitTests.Services;

public sealed class EmployeeServiceTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateEmployeeService()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var assignedAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var employeeService = EmployeeService.Create(
            Guid.NewGuid(),
            organizationId,
            employeeId,
            serviceId,
            assignedAtUtc);

        Assert.Equal(
            organizationId,
            employeeService.OrganizationId);

        Assert.Equal(
            employeeId,
            employeeService.EmployeeId);

        Assert.Equal(
            serviceId,
            employeeService.ServiceId);

        Assert.Equal(
            assignedAtUtc,
            employeeService.AssignedAtUtc);
    }

    [Fact]
    public void CreateWithEmptyEmployeeIdShouldThrowArgumentException()
    {
        var action = () => EmployeeService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyServiceIdShouldThrowArgumentException()
    {
        var action = () => EmployeeService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }
}
