namespace BookingHub.Application.Employees.Schedules.CreateWorkingHours;

public sealed record CreateWorkingHoursCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
