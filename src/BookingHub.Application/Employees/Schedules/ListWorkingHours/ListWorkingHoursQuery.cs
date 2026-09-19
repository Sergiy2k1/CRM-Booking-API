namespace BookingHub.Application.Employees.Schedules.ListWorkingHours;

public sealed record ListWorkingHoursQuery(
    Guid OrganizationId,
    Guid EmployeeId);
