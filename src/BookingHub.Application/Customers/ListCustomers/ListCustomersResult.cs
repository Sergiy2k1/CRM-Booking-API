using BookingHub.Application.Customers.Common;

namespace BookingHub.Application.Customers.ListCustomers;

public sealed record ListCustomersResult(
    IReadOnlyCollection<CustomerDetails> Items,
    int Page,
    int PageSize,
    int TotalCount);
