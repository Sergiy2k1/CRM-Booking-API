namespace BookingHub.Application.Customers.ArchiveCustomer;

public sealed record ArchiveCustomerCommand(
    Guid OrganizationId,
    Guid CustomerId);
