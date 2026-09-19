namespace BookingHub.Application.Services.Assignments.AssignService;

public sealed record AssignServiceCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    Guid ServiceId);
