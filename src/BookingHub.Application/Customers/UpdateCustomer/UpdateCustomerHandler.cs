using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Customers.Common;
using BookingHub.Domain.Customers;

namespace BookingHub.Application.Customers.UpdateCustomer;

public sealed class UpdateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerHandler(
        ICustomerRepository customerRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDetails> HandleAsync(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customer =
            await GetCustomerForUpdateAsync(
                command.OrganizationId,
                command.CustomerId,
                cancellationToken);

        var utcNow =
            _clock.UtcNow;

        customer.UpdateName(
            command.FirstName,
            command.LastName,
            utcNow);

        customer.UpdateContactDetails(
            command.Email,
            command.Phone,
            utcNow);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return customer.ToDetails();
    }

    private async Task<Customer> GetCustomerForUpdateAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        ValidateId(organizationId);
        ValidateId(customerId);

        var customer =
            await _customerRepository.GetTrackedByOrganizationAndIdAsync(
                organizationId,
                customerId,
                cancellationToken);

        if (customer is null)
        {
            throw new EntityNotFoundException(
                nameof(Customer),
                customerId);
        }

        return customer;
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                nameof(id));
        }
    }
}
