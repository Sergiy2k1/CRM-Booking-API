using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;

namespace BookingHub.Application.Employees.Schedules.CancelTimeOff;

public sealed class CancelTimeOffHandler
{
    private readonly IEmployeeScheduleRepository _scheduleRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CancelTimeOffHandler(
        IEmployeeScheduleRepository scheduleRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _scheduleRepository = scheduleRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<TimeOffDetails> HandleAsync(
        CancelTimeOffCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var timeOff =
            await _scheduleRepository.GetTrackedTimeOffByIdAsync(
                command.OrganizationId,
                command.EmployeeId,
                command.TimeOffId,
                cancellationToken);

        if (timeOff is null)
        {
            throw new EntityNotFoundException(
                nameof(EmployeeTimeOff),
                command.TimeOffId);
        }

        timeOff.Cancel(_clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

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
