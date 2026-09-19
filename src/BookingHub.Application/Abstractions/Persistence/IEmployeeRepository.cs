using BookingHub.Domain.Employees;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Employee>> ListAsync(
        Guid organizationId,
        EmployeeStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid organizationId,
        EmployeeStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default);
}
