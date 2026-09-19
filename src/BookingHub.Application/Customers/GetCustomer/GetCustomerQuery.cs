namespace BookingHub.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(
    Guid OrganizationId,
    Guid CustomerId);
