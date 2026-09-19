using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
