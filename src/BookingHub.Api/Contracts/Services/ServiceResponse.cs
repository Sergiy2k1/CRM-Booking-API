using BookingHub.Domain.Services;

namespace BookingHub.Api.Contracts.Services;

public sealed record ServiceResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    TimeSpan Duration,
    decimal PriceAmount,
    string Currency,
    ServiceStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
