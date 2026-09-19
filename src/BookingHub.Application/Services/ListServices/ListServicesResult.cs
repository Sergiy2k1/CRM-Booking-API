using BookingHub.Application.Services.Common;

namespace BookingHub.Application.Services.ListServices;

public sealed record ListServicesResult(
    IReadOnlyCollection<ServiceDetails> Items,
    int Page,
    int PageSize,
    int TotalCount);
