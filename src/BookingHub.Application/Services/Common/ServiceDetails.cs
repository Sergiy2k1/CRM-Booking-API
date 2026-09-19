using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.Common;

public sealed record ServiceDetails(
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
