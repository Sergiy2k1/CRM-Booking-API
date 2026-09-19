using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Schedules.CreateWorkingHours;

public sealed class CreateWorkingHoursHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeScheduleRepository _scheduleRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkingHoursHandler(
        IEmployeeRepository employeeRepository,
        IEmployeeScheduleRepository scheduleRepository,
        IGuidGenerator guidGenerator,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _scheduleRepository = scheduleRepository;
        _guidGenerator = guidGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkingHoursDetails> HandleAsync(
        CreateWorkingHoursCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await EnsureActiveEmployeeAsync(
            command.OrganizationId,
            command.EmployeeId,
            cancellationToken);

        var hasOverlap =
            await _scheduleRepository.HasWorkingHoursOverlapAsync(
                command.OrganizationId,
                command.EmployeeId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime,
                cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "Working hours overlap an existing schedule period.");
        }

        var workingHours =
            EmployeeWorkingHours.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.EmployeeId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime);

        await _scheduleRepository.AddWorkingHoursAsync(
            workingHours,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Map(workingHours);
    }

    private async Task EnsureActiveEmployeeAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        ValidateId(organizationId);
        ValidateId(employeeId);

        var employee =
            await _employeeRepository.GetByOrganizationAndIdAsync(
                organizationId,
                employeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                employeeId);
        }

        if (employee.Status != EmployeeStatus.Active)
        {
            throw new InvalidOperationException(
                "Working hours cannot be added to an inactive employee.");
        }
    }

    private static WorkingHoursDetails Map(
        EmployeeWorkingHours workingHours)
    {
        return new WorkingHoursDetails(
            workingHours.Id,
            workingHours.OrganizationId,
            workingHours.EmployeeId,
            workingHours.DayOfWeek,
            workingHours.StartTime,
            workingHours.EndTime);
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
