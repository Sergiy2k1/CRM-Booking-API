using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Services.Common;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.CreateService;

public sealed class CreateServiceHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceHandler(
        IOrganizationRepository organizationRepository,
        IServiceRepository serviceRepository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _serviceRepository = serviceRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDetails> HandleAsync(
        CreateServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateOrganizationId(command.OrganizationId);

        var organization =
            await _organizationRepository.GetByIdAsync(
                command.OrganizationId,
                cancellationToken);

        if (organization is null)
        {
            throw new EntityNotFoundException(
                nameof(Organization),
                command.OrganizationId);
        }

        if (organization.Status != OrganizationStatus.Active)
        {
            throw new InvalidOperationException(
                "Service cannot be created for a suspended organization.");
        }

        var service =
            Service.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.Name,
                command.Description,
                command.Duration,
                command.PriceAmount,
                command.Currency,
                _clock.UtcNow);

        await _serviceRepository.AddAsync(
            service,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return service.ToDetails();
    }

    private static void ValidateOrganizationId(
        Guid organizationId)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id cannot be empty.",
                nameof(organizationId));
        }
    }
}
