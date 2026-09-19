namespace BookingHub.Api.Contracts.Employees;

public sealed record CreateWorkingHoursRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
