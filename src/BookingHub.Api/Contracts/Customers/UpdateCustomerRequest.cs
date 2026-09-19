namespace BookingHub.Api.Contracts.Customers;

public sealed record UpdateCustomerRequest(
    string FirstName,
    string? LastName,
    string? Email,
    string? Phone);
