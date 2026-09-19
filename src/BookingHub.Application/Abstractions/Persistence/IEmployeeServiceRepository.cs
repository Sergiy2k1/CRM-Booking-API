using BookingHub.Domain.Services;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IEmployeeServiceRepository
{
    Task<bool> IsAssignedAsync(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EmployeeService>> ListByEmployeeAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<EmployeeService?> GetTrackedAsync(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        EmployeeService assignment,
        CancellationToken cancellationToken = default);

    void Remove(EmployeeService assignment);
}
