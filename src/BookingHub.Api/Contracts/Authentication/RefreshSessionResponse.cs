using BookingHub.Domain.Organizations;

namespace BookingHub.Api.Contracts.Authentication;

public sealed record RefreshSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role);
