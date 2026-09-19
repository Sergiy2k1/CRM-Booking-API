namespace BookingHub.Application.Employees.CreateEmployee;

public sealed record CreateEmployeeCommand(
    Guid OrganizationId,
    string FirstName,
    string? LastName,
    string? Position);
