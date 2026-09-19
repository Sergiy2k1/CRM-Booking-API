using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Schedules.CreateTimeOff;

public sealed class CreateTimeOffHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeScheduleRepository _scheduleRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTimeOffHandler(
        IEmployeeRepository employeeRepository,
        IEmployeeScheduleRepository scheduleRepository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _scheduleRepository = scheduleRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<TimeOffDetails> HandleAsync(
        CreateTimeOffCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var employee =
            await _employeeRepository.GetByOrganizationAndIdAsync(
                command.OrganizationId,
                command.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                command.EmployeeId);
        }

        if (employee.Status != EmployeeStatus.Active)
        {
            throw new InvalidOperationException(
                "Time off cannot be added to an inactive employee.");
        }

        var timeOff =
            EmployeeTimeOff.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.EmployeeId,
                command.StartsAtUtc,
                command.EndsAtUtc,
                command.Reason,
                _clock.UtcNow);

        await _scheduleRepository.AddTimeOffAsync(
            timeOff,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Map(timeOff);
    }

    private static TimeOffDetails Map(
        EmployeeTimeOff timeOff)
    {
        return new TimeOffDetails(
            timeOff.Id,
            timeOff.OrganizationId,
            timeOff.EmployeeId,
            timeOff.StartsAtUtc,
            timeOff.EndsAtUtc,
            timeOff.Reason,
            timeOff.Status,
            timeOff.CreatedAtUtc,
            timeOff.UpdatedAtUtc,
            timeOff.CancelledAtUtc);
    }
}
