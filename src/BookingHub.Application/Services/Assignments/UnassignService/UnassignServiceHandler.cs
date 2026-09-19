using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.Assignments.UnassignService;

public sealed class UnassignServiceHandler
{
    private readonly IEmployeeServiceRepository _employeeServiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignServiceHandler(
        IEmployeeServiceRepository employeeServiceRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeServiceRepository = employeeServiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        UnassignServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var assignment =
            await _employeeServiceRepository.GetTrackedAsync(
                command.OrganizationId,
                command.EmployeeId,
                command.ServiceId,
                cancellationToken);

        if (assignment is null)
        {
            throw new EntityNotFoundException(
                nameof(EmployeeService),
                command.ServiceId);
        }

        _employeeServiceRepository.Remove(
            assignment);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
