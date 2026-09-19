namespace BookingHub.Application.Employees.Common;

public sealed record WorkingHoursDetails(
    Guid Id,
    Guid OrganizationId,
    Guid EmployeeId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
