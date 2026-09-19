namespace BookingHub.Api.Contracts.Employees;

public sealed record UpdateEmployeeRequest(
    string FirstName,
    string? LastName,
    string? Position);
