using BookingHub.Domain.Employees;

namespace BookingHub.Api.Contracts.Employees;

public sealed record TimeOffResponse(
    Guid Id,
    Guid OrganizationId,
    Guid EmployeeId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason,
    EmployeeTimeOffStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CancelledAtUtc);
