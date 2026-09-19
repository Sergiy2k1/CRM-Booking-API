namespace BookingHub.Application.Customers.RestoreCustomer;

public sealed record RestoreCustomerCommand(
    Guid OrganizationId,
    Guid CustomerId);
