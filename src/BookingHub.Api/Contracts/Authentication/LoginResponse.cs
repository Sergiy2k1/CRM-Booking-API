using BookingHub.Domain.Organizations;

namespace BookingHub.Api.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role);
