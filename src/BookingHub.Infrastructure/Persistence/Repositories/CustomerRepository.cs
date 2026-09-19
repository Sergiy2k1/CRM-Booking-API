using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository
    : ICustomerRepository
{
    private readonly BookingHubDbContext _dbContext;

    public CustomerRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}
