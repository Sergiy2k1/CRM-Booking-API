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
}
