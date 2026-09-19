using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository
    : IServiceRepository
{
    private readonly BookingHubDbContext _dbContext;

    public ServiceRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Service?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Services
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<Service?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Services
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == serviceId,
                cancellationToken);
    }

    public Task<Service?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Services
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == serviceId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Service>> ListAsync(
        Guid organizationId,
        ServiceStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Services
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
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid organizationId,
        ServiceStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Services
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
        Service service,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Services.AddAsync(
            service,
            cancellationToken);
    }
}
