namespace BookingHub.Application.Services.Assignments.ListEmployeeServices;

public sealed record ListEmployeeServicesQuery(
    Guid OrganizationId,
    Guid EmployeeId);
