using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Abstractions.Authentication;

public interface IAccessTokenProvider
{
    AccessToken Create(
        Guid userId,
        Guid organizationId,
        string email,
        OrganizationRole role,
        DateTimeOffset issuedAtUtc);
}
