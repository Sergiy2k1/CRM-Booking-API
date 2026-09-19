using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Authentication.Login;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role);
