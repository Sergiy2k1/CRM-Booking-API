using BookingHub.Application.Employees.Common;

namespace BookingHub.Application.Employees.ListEmployees;

public sealed record ListEmployeesResult(
    IReadOnlyCollection<EmployeeDetails> Items,
    int Page,
    int PageSize,
    int TotalCount);
