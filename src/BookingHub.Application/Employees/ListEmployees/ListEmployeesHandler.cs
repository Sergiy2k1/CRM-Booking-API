using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Employees.Common;

namespace BookingHub.Application.Employees.ListEmployees;

public sealed class ListEmployeesHandler
{
    public const int MaxPageSize = 100;

    private readonly IEmployeeRepository _employeeRepository;

    public ListEmployeesHandler(
        IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<ListEmployeesResult> HandleAsync(
        ListEmployeesQuery query,
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
                "Employee status is invalid.");
        }

        var skip =
            (query.Page - 1) * query.PageSize;

        var employees =
            await _employeeRepository.ListAsync(
                query.OrganizationId,
                query.Status,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _employeeRepository.CountAsync(
                query.OrganizationId,
                query.Status,
                cancellationToken);

        return new ListEmployeesResult(
            employees
                .Select(employee => employee.ToDetails())
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
