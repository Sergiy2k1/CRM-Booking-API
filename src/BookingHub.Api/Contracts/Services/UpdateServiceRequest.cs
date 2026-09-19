namespace BookingHub.Api.Contracts.Services;

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    TimeSpan Duration,
    decimal PriceAmount,
    string Currency);
