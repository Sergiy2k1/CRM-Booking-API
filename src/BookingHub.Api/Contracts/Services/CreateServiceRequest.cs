namespace BookingHub.Api.Contracts.Services;

public sealed record CreateServiceRequest(
    string Name,
    string? Description,
    TimeSpan Duration,
    decimal PriceAmount,
    string Currency);
