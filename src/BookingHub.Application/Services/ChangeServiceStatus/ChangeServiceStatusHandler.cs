using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Services.Common;
using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.ChangeServiceStatus;

public sealed class ChangeServiceStatusHandler
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeServiceStatusHandler(
        IServiceRepository serviceRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDetails> HandleAsync(
        ChangeServiceStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var service =
            await _serviceRepository.GetTrackedByOrganizationAndIdAsync(
                command.OrganizationId,
                command.ServiceId,
                cancellationToken);

        if (service is null)
        {
            throw new EntityNotFoundException(
                nameof(Service),
                command.ServiceId);
        }

        if (command.Activate)
        {
            service.Activate(_clock.UtcNow);
        }
        else
        {
            service.Deactivate(_clock.UtcNow);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return service.ToDetails();
    }
}
