namespace BookingHub.Application.Employees.GetEmployee;

public sealed record GetEmployeeQuery(
    Guid OrganizationId,
    Guid EmployeeId);
