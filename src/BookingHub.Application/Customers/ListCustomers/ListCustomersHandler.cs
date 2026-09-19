using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Customers.Common;

namespace BookingHub.Application.Customers.ListCustomers;

public sealed class ListCustomersHandler
{
    public const int MaxPageSize = 100;

    private readonly ICustomerRepository _customerRepository;

    public ListCustomersHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ListCustomersResult> HandleAsync(
        ListCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateOrganizationId(query.OrganizationId);
        ValidatePagination(query.Page, query.PageSize);

        if (query.Status.HasValue &&
            !Enum.IsDefined(query.Status.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Customer status is invalid.");
        }

        var skip =
            (query.Page - 1) * query.PageSize;

        var customers =
            await _customerRepository.ListAsync(
                query.OrganizationId,
                query.Status,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _customerRepository.CountAsync(
                query.OrganizationId,
                query.Status,
                cancellationToken);

        return new ListCustomersResult(
            customers
                .Select(customer => customer.ToDetails())
                .ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
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

    private static void ValidatePagination(
        int page,
        int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                page,
                "Page must be at least 1.");
        }

        if (pageSize < 1 ||
            pageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                $"Page size must be between 1 and {MaxPageSize}.");
        }
    }
}
