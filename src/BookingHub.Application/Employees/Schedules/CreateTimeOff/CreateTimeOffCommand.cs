namespace BookingHub.Application.Employees.Schedules.CreateTimeOff;

public sealed record CreateTimeOffCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason);
