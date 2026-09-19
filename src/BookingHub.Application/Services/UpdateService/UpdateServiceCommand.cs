namespace BookingHub.Application.Services.UpdateService;

public sealed record UpdateServiceCommand(
    Guid OrganizationId,
    Guid ServiceId,
    string Name,
    string? Description,
    TimeSpan Duration,
    decimal PriceAmount,
    string Currency);
