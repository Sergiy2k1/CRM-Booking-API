using BookingHub.Domain.Customers;

namespace BookingHub.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Customer>> ListAsync(
        Guid organizationId,
        CustomerStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid organizationId,
        CustomerStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default);
}
