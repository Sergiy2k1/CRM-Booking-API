namespace BookingHub.Application.Employees.UpdateEmployee;

public sealed record UpdateEmployeeCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    string FirstName,
    string? LastName,
    string? Position);
