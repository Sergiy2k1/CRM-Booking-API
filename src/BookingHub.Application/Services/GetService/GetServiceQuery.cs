namespace BookingHub.Application.Services.GetService;

public sealed record GetServiceQuery(
    Guid OrganizationId,
    Guid ServiceId);
