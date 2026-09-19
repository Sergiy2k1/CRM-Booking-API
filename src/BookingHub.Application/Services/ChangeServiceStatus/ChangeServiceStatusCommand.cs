namespace BookingHub.Application.Services.ChangeServiceStatus;

public sealed record ChangeServiceStatusCommand(
    Guid OrganizationId,
    Guid ServiceId,
    bool Activate);
