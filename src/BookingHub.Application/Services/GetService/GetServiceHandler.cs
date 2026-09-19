using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Services.Common;
using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.GetService;

public sealed class GetServiceHandler
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceHandler(
        IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceDetails> HandleAsync(
        GetServiceQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var service =
            await _serviceRepository.GetByOrganizationAndIdAsync(
                query.OrganizationId,
                query.ServiceId,
                cancellationToken);

        if (service is null)
        {
            throw new EntityNotFoundException(
                nameof(Service),
                query.ServiceId);
        }

        return service.ToDetails();
    }
}
