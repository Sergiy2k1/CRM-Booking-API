namespace BookingHub.Api.Contracts.Employees;

public sealed record CreateTimeOffRequest(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason);
