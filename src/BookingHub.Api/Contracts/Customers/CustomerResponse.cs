using BookingHub.Domain.Customers;

namespace BookingHub.Api.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    Guid OrganizationId,
    string FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    CustomerStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ArchivedAtUtc);
