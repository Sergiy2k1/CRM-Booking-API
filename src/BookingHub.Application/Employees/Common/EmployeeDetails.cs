using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Common;

public sealed record EmployeeDetails(
    Guid Id,
    Guid OrganizationId,
    Guid? UserId,
    string FirstName,
    string? LastName,
    string? Position,
    EmployeeStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
