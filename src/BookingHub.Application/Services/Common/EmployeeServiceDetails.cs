namespace BookingHub.Application.Services.Common;

public sealed record EmployeeServiceDetails(
    Guid Id,
    Guid OrganizationId,
    Guid EmployeeId,
    Guid ServiceId,
    DateTimeOffset AssignedAtUtc);
