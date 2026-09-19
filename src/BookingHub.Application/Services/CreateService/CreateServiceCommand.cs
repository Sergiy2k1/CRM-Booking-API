namespace BookingHub.Application.Services.CreateService;

public sealed record CreateServiceCommand(
    Guid OrganizationId,
    string Name,
    string? Description,
    TimeSpan Duration,
    decimal PriceAmount,
    string Currency);
