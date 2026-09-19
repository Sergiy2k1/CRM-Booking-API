using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Customers.Common;
using BookingHub.Domain.Customers;

namespace BookingHub.Application.Customers.RestoreCustomer;

public sealed class RestoreCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreCustomerHandler(
        ICustomerRepository customerRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDetails> HandleAsync(
        RestoreCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customer =
            await GetCustomerForUpdateAsync(
                command.OrganizationId,
                command.CustomerId,
                cancellationToken);

        customer.Restore(_clock.UtcNow);

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
