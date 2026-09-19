using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.ListEmployees;

public sealed record ListEmployeesQuery(
    Guid OrganizationId,
    EmployeeStatus? Status,
    int Page = 1,
    int PageSize = 20);
