using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.ChangeEmployeeStatus;

public sealed class ChangeEmployeeStatusHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeEmployeeStatusHandler(
        IEmployeeRepository employeeRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDetails> HandleAsync(
        ChangeEmployeeStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateId(command.OrganizationId);
        ValidateId(command.EmployeeId);

        var employee =
            await _employeeRepository.GetTrackedByOrganizationAndIdAsync(
                command.OrganizationId,
                command.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                command.EmployeeId);
        }

        if (command.Activate)
        {
            employee.Activate(_clock.UtcNow);
        }
        else
        {
            employee.Deactivate(_clock.UtcNow);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

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
