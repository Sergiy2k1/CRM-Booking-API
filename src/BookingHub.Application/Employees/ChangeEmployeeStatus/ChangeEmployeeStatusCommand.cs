namespace BookingHub.Application.Employees.ChangeEmployeeStatus;

public sealed record ChangeEmployeeStatusCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    bool Activate);
