using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Common;

internal static class EmployeeMappings
{
    public static EmployeeDetails ToDetails(
        this Employee employee)
    {
        return new EmployeeDetails(
            employee.Id,
            employee.OrganizationId,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            employee.Position,
            employee.Status,
            employee.CreatedAtUtc,
            employee.UpdatedAtUtc);
    }
}
