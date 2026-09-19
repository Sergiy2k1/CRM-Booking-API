using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.Common;

internal static class ServiceMappings
{
    public static ServiceDetails ToDetails(
        this Service service)
    {
        return new ServiceDetails(
            service.Id,
            service.OrganizationId,
            service.Name,
            service.Description,
            service.Duration,
            service.PriceAmount,
            service.Currency,
            service.Status,
            service.CreatedAtUtc,
            service.UpdatedAtUtc);
    }
}
