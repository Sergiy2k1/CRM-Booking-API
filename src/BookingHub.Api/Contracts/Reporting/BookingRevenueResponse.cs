namespace BookingHub.Api.Contracts.Reporting;

public sealed record BookingRevenueResponse(
    string Currency,
    decimal Amount);
