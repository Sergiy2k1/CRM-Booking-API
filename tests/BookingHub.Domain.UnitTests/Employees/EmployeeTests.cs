using BookingHub.Domain.Employees;
using Xunit;

namespace BookingHub.Domain.UnitTests.Employees;

public sealed class EmployeeTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveEmployee()
    {
        var id = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var employee = Employee.Create(
            id,
            organizationId,
            userId,
            "Sergiy",
            "Tester",
            "Barber",
            createdAtUtc);

        Assert.Equal(id, employee.Id);
        Assert.Equal(organizationId, employee.OrganizationId);
        Assert.Equal(userId, employee.UserId);
        Assert.Equal("Sergiy", employee.FirstName);
        Assert.Equal("Tester", employee.LastName);
        Assert.Equal("Barber", employee.Position);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.Equal(createdAtUtc, employee.CreatedAtUtc);
        Assert.Equal(createdAtUtc, employee.UpdatedAtUtc);
    }

    [Fact]
    public void CreateWithoutUserShouldCreateBookableEmployee()
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Sergiy",
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.Null(employee.UserId);
        Assert.Null(employee.LastName);
        Assert.Null(employee.Position);
    }

    [Fact]
    public void CreateWithEmptyOrganizationIdShouldThrowArgumentException()
    {
        var action = () => Employee.Create(
            Guid.NewGuid(),
            Guid.Empty,
            null,
            "Sergiy",
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void LinkUserShouldAssignUserId()
    {
        var employee = CreateEmployee();
        var userId = Guid.NewGuid();
        var updatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(10);

        employee.LinkUser(
            userId,
            updatedAtUtc);

        Assert.Equal(userId, employee.UserId);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            employee.UpdatedAtUtc);
    }

    [Fact]
    public void UnlinkUserShouldClearUserId()
    {
        var employee = CreateEmployee(Guid.NewGuid());

        employee.UnlinkUser(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Null(employee.UserId);
    }

    [Fact]
    public void DeactivateShouldMakeEmployeeInactive()
    {
        var employee = CreateEmployee();

        employee.Deactivate(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Equal(
            EmployeeStatus.Inactive,
            employee.Status);
    }

    [Fact]
    public void UpdateInactiveEmployeeShouldThrowInvalidOperationException()
    {
        var employee = CreateEmployee();

        employee.Deactivate(
            DateTimeOffset.UtcNow);

        var action = () => employee.UpdateProfile(
            "New",
            "Name",
            "Manager",
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Throws<InvalidOperationException>(
            action);
    }

    private static Employee CreateEmployee(
        Guid? userId = null)
    {
        return Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            "Sergiy",
            "Tester",
            "Barber",
            DateTimeOffset.UtcNow);
    }
}
