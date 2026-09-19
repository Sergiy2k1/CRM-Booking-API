namespace BookingHub.Api.Contracts.Customers;

public sealed record CustomerListResponse(
    IReadOnlyCollection<CustomerResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
