using BookingHub.Domain.Services;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Service?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<Service?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Service>> ListAsync(
        Guid organizationId,
        ServiceStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid organizationId,
        ServiceStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Service service,
        CancellationToken cancellationToken = default);
}
