using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationRepository
    : IOrganizationRepository
{
    private readonly BookingHubDbContext _dbContext;

    public OrganizationRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Organization?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}
