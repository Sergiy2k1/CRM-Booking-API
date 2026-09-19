using BookingHub.Domain.Customers;

namespace BookingHub.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
