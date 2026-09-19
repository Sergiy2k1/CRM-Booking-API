namespace BookingHub.Api.Contracts.Employees;

public sealed record EmployeeListResponse(
    IReadOnlyCollection<EmployeeResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
