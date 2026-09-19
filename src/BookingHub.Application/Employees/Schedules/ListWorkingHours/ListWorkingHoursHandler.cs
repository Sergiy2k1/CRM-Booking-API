using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Schedules.ListWorkingHours;

public sealed class ListWorkingHoursHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeScheduleRepository _scheduleRepository;

    public ListWorkingHoursHandler(
        IEmployeeRepository employeeRepository,
        IEmployeeScheduleRepository scheduleRepository)
    {
        _employeeRepository = employeeRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyCollection<WorkingHoursDetails>> HandleAsync(
        ListWorkingHoursQuery query,
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

        var workingHours =
            await _scheduleRepository.GetWorkingHoursAsync(
                query.OrganizationId,
                query.EmployeeId,
                cancellationToken);

        return workingHours
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .Select(
                x => new WorkingHoursDetails(
                    x.Id,
                    x.OrganizationId,
                    x.EmployeeId,
                    x.DayOfWeek,
                    x.StartTime,
                    x.EndTime))
            .ToArray();
    }
}
