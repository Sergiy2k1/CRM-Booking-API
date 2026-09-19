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

    public Task<Customer?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == customerId,
                cancellationToken);
    }

    public Task<Customer?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == customerId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Customer>> ListAsync(
        Guid organizationId,
        CustomerStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Customers
                .AsNoTracking()
                .Where(
                    x => x.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query =
                query.Where(
                    x => x.Status == status.Value);
        }

        return await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid organizationId,
        CustomerStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Customers
                .AsNoTracking()
                .Where(
                    x => x.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query =
                query.Where(
                    x => x.Status == status.Value);
        }

        return query.CountAsync(
            cancellationToken);
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(
            customer,
            cancellationToken);
    }
}
