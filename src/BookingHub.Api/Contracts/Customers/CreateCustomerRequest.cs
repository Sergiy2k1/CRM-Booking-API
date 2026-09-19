namespace BookingHub.Api.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string FirstName,
    string? LastName,
    string? Email,
    string? Phone);
