using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Authentication.RefreshSession;

public sealed record RefreshSessionResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role);
