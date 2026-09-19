namespace BookingHub.Api.Contracts.Services;

public sealed record EmployeeServiceResponse(
    Guid Id,
    Guid OrganizationId,
    Guid EmployeeId,
    Guid ServiceId,
    DateTimeOffset AssignedAtUtc);
