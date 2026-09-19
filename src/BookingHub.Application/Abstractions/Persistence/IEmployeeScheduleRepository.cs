using BookingHub.Domain.Employees;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IEmployeeScheduleRepository
{
    Task<IReadOnlyCollection<EmployeeWorkingHours>> GetWorkingHoursAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<bool> HasWorkingHoursOverlapAsync(
        Guid organizationId,
        Guid employeeId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default);

    Task AddWorkingHoursAsync(
        EmployeeWorkingHours workingHours,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EmployeeTimeOff>> GetTimeOffAsync(
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default);

    Task<EmployeeTimeOff?> GetTrackedTimeOffByIdAsync(
        Guid organizationId,
        Guid employeeId,
        Guid timeOffId,
        CancellationToken cancellationToken = default);

    Task AddTimeOffAsync(
        EmployeeTimeOff timeOff,
        CancellationToken cancellationToken = default);
}
