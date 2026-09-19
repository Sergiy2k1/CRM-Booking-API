using BookingHub.Domain.Employees;

namespace BookingHub.Api.Contracts.Employees;

public sealed record EmployeeResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? UserId,
    string FirstName,
    string? LastName,
    string? Position,
    EmployeeStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
