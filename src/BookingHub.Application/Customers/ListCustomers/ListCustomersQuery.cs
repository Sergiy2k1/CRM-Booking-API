using BookingHub.Domain.Customers;

namespace BookingHub.Application.Customers.ListCustomers;

public sealed record ListCustomersQuery(
    Guid OrganizationId,
    CustomerStatus? Status,
    int Page = 1,
    int PageSize = 20);
