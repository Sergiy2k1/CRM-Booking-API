namespace BookingHub.Application.Abstractions.Authentication;

public sealed record GeneratedRefreshToken(
    string Value,
    string Hash,
    DateTimeOffset ExpiresAtUtc);
