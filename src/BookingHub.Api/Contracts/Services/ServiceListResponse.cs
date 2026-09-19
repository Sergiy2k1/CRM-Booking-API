namespace BookingHub.Api.Contracts.Services;

public sealed record ServiceListResponse(
    IReadOnlyCollection<ServiceResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
