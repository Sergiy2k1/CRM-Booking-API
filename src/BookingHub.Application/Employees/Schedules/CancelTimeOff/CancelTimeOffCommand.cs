namespace BookingHub.Application.Employees.Schedules.CancelTimeOff;

public sealed record CancelTimeOffCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    Guid TimeOffId);
