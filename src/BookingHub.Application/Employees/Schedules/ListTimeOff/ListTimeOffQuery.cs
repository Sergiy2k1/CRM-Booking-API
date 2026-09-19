namespace BookingHub.Application.Employees.Schedules.ListTimeOff;

public sealed record ListTimeOffQuery(
    Guid OrganizationId,
    Guid EmployeeId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
