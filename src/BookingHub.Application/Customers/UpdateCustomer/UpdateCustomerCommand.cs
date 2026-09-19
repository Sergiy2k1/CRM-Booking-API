namespace BookingHub.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid OrganizationId,
    Guid CustomerId,
    string FirstName,
    string? LastName,
    string? Email,
    string? Phone);
