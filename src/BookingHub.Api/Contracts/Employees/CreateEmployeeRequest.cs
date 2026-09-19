namespace BookingHub.Api.Contracts.Employees;

public sealed record CreateEmployeeRequest(
    string FirstName,
    string? LastName,
    string? Position);
