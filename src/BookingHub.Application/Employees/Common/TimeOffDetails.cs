using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Common;

public sealed record TimeOffDetails(
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
