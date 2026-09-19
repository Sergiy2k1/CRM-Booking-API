namespace BookingHub.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    Guid OrganizationId,
    string FirstName,
    string? LastName,
    string? Email,
    string? Phone);
