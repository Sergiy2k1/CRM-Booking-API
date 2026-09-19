using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Customers.Common;
using BookingHub.Domain.Customers;

namespace BookingHub.Application.Customers.GetCustomer;

public sealed class GetCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDetails> HandleAsync(
        GetCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateId(query.OrganizationId);
        ValidateId(query.CustomerId);

        var customer =
            await _customerRepository.GetByOrganizationAndIdAsync(
                query.OrganizationId,
                query.CustomerId,
                cancellationToken);

        if (customer is null)
        {
            throw new EntityNotFoundException(
                nameof(Customer),
                query.CustomerId);
        }

        return customer.ToDetails();
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
