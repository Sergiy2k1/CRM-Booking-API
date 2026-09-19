using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Customers.Common;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Customers.CreateCustomer;

public sealed class CreateCustomerHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerHandler(
        IOrganizationRepository organizationRepository,
        ICustomerRepository customerRepository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _customerRepository = customerRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDetails> HandleAsync(
        CreateCustomerCommand command,
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
                "Customer cannot be created for a suspended organization.");
        }

        var customer =
            Customer.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Phone,
                _clock.UtcNow);

        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return customer.ToDetails();
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
