using BookingHub.Domain.Customers;

namespace BookingHub.Application.Customers.Common;

internal static class CustomerMappings
{
    public static CustomerDetails ToDetails(
        this Customer customer)
    {
        return new CustomerDetails(
            customer.Id,
            customer.OrganizationId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.Status,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc,
            customer.ArchivedAtUtc);
    }
}
