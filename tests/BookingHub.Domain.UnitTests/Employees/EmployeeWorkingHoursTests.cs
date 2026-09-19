using BookingHub.Domain.Employees;
using Xunit;

namespace BookingHub.Domain.UnitTests.Employees;

public sealed class EmployeeWorkingHoursTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateWorkingHours()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var workingHours = EmployeeWorkingHours.Create(
            Guid.NewGuid(),
            organizationId,
            employeeId,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));

        Assert.Equal(
            organizationId,
            workingHours.OrganizationId);

        Assert.Equal(
            employeeId,
            workingHours.EmployeeId);

        Assert.Equal(
            DayOfWeek.Monday,
            workingHours.DayOfWeek);

        Assert.Equal(
            new TimeOnly(9, 0),
            workingHours.StartTime);

        Assert.Equal(
            new TimeOnly(18, 0),
            workingHours.EndTime);
    }

    [Fact]
    public void CreateWithEmptyEmployeeIdShouldThrowArgumentException()
    {
        var action = () => EmployeeWorkingHours.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEndBeforeStartShouldThrowArgumentException()
    {
        var action = () => EmployeeWorkingHours.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(18, 0),
            new TimeOnly(9, 0));

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void UpdateHoursShouldChangeSchedule()
    {
        var workingHours = CreateWorkingHours();

        workingHours.UpdateHours(
            DayOfWeek.Tuesday,
            new TimeOnly(10, 0),
            new TimeOnly(19, 0));

        Assert.Equal(
            DayOfWeek.Tuesday,
            workingHours.DayOfWeek);

        Assert.Equal(
            new TimeOnly(10, 0),
            workingHours.StartTime);

        Assert.Equal(
            new TimeOnly(19, 0),
            workingHours.EndTime);
    }

    private static EmployeeWorkingHours CreateWorkingHours()
    {
        return EmployeeWorkingHours.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));
    }
}
