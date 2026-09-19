using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Services.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Services.Assignments.ListEmployeeServices;

public sealed class ListEmployeeServicesHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeServiceRepository _employeeServiceRepository;

    public ListEmployeeServicesHandler(
        IEmployeeRepository employeeRepository,
        IEmployeeServiceRepository employeeServiceRepository)
    {
        _employeeRepository = employeeRepository;
        _employeeServiceRepository = employeeServiceRepository;
    }

    public async Task<IReadOnlyCollection<EmployeeServiceDetails>> HandleAsync(
        ListEmployeeServicesQuery query,
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

        var assignments =
            await _employeeServiceRepository.ListByEmployeeAsync(
                query.OrganizationId,
                query.EmployeeId,
                cancellationToken);

        return assignments
            .OrderBy(x => x.AssignedAtUtc)
            .Select(
                x => new EmployeeServiceDetails(
                    x.Id,
                    x.OrganizationId,
                    x.EmployeeId,
                    x.ServiceId,
                    x.AssignedAtUtc))
            .ToArray();
    }
}
