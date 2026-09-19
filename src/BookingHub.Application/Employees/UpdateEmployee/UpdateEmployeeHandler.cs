using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.UpdateEmployee;

public sealed class UpdateEmployeeHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeHandler(
        IEmployeeRepository employeeRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDetails> HandleAsync(
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var employee =
            await GetEmployeeForUpdateAsync(
                command.OrganizationId,
                command.EmployeeId,
                cancellationToken);

        employee.UpdateProfile(
            command.FirstName,
            command.LastName,
            command.Position,
            _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return employee.ToDetails();
    }

    private async Task<Employee> GetEmployeeForUpdateAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        ValidateId(organizationId);
        ValidateId(employeeId);

        var employee =
            await _employeeRepository.GetTrackedByOrganizationAndIdAsync(
                organizationId,
                employeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                employeeId);
        }

        return employee;
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
