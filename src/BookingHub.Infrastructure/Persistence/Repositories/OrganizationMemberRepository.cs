using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationMemberRepository
    : IOrganizationMemberRepository
{
    private readonly BookingHubDbContext _dbContext;

    public OrganizationMemberRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OrganizationMember?> GetByOrganizationAndUserAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganizationMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == userId,
                cancellationToken);
    }
}
