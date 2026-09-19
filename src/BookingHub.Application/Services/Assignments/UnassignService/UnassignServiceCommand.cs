namespace BookingHub.Application.Services.Assignments.UnassignService;

public sealed record UnassignServiceCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    Guid ServiceId);
