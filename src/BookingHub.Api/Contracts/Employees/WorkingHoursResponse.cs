namespace BookingHub.Api.Contracts.Employees;

public sealed record WorkingHoursResponse(
    Guid Id,
    Guid OrganizationId,
    Guid EmployeeId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
