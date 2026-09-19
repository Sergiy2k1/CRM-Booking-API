using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Schedules.ListTimeOff;

public sealed class ListTimeOffHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeScheduleRepository _scheduleRepository;

    public ListTimeOffHandler(
        IEmployeeRepository employeeRepository,
        IEmployeeScheduleRepository scheduleRepository)
    {
        _employeeRepository = employeeRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyCollection<TimeOffDetails>> HandleAsync(
        ListTimeOffQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var employee =
            await _employeeRepository.GetByOrganizationAndIdAsync(
                query.OrganizationId,
                query.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                query.EmployeeId);
        }

        if (query.EndsAtUtc <= query.StartsAtUtc)
        {
            throw new ArgumentException(
                "Time range end must be later than start.",
                nameof(query));
        }

        var timeOff =
            await _scheduleRepository.GetTimeOffAsync(
                query.OrganizationId,
                query.EmployeeId,
                query.StartsAtUtc.ToUniversalTime(),
                query.EndsAtUtc.ToUniversalTime(),
                cancellationToken);

        return timeOff
            .OrderBy(x => x.StartsAtUtc)
            .Select(
                x => new TimeOffDetails(
                    x.Id,
                    x.OrganizationId,
                    x.EmployeeId,
                    x.StartsAtUtc,
                    x.EndsAtUtc,
                    x.Reason,
                    x.Status,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc,
                    x.CancelledAtUtc))
            .ToArray();
    }
}
