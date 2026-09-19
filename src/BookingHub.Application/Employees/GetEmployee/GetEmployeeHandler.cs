using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.GetEmployee;

public sealed class GetEmployeeHandler
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeeHandler(
        IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDetails> HandleAsync(
        GetEmployeeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateId(query.OrganizationId);
        ValidateId(query.EmployeeId);

        var employee =
            await _employeeRepository.GetByOrganizationAndIdAsync(
                query.OrganizationId,
                query.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                query.EmployeeId);
        }

        return employee.ToDetails();
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
