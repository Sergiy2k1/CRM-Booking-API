using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IOrganizationMemberRepository
{
    Task<OrganizationMember?> GetByOrganizationAndUserAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
