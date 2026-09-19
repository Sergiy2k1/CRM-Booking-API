using BookingHub.Domain.Employees;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IEmployeeScheduleRepository
{
    Task<IReadOnlyCollection<EmployeeWorkingHours>> GetWorkingHoursAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EmployeeTimeOff>> GetTimeOffAsync(
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default);
}
