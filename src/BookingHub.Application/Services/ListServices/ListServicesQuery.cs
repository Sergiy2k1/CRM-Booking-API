using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.ListServices;

public sealed record ListServicesQuery(
    Guid OrganizationId,
    ServiceStatus? Status,
    int Page = 1,
    int PageSize = 20);
