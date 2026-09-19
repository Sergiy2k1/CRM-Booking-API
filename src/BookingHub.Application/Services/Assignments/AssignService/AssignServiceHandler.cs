using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Services.Common;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Services;

namespace BookingHub.Application.Services.Assignments.AssignService;

public sealed class AssignServiceHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IEmployeeServiceRepository _employeeServiceRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public AssignServiceHandler(
        IEmployeeRepository employeeRepository,
        IServiceRepository serviceRepository,
        IEmployeeServiceRepository employeeServiceRepository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _serviceRepository = serviceRepository;
        _employeeServiceRepository = employeeServiceRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeServiceDetails> HandleAsync(
        AssignServiceCommand command,
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
                "Service cannot be assigned to an inactive employee.");
        }

        var service =
            await _serviceRepository.GetByOrganizationAndIdAsync(
                command.OrganizationId,
                command.ServiceId,
                cancellationToken);

        if (service is null)
        {
            throw new EntityNotFoundException(
                nameof(Service),
                command.ServiceId);
        }

        if (service.Status != ServiceStatus.Active)
        {
            throw new InvalidOperationException(
                "Inactive service cannot be assigned.");
        }

        if (await _employeeServiceRepository.IsAssignedAsync(
                command.OrganizationId,
                command.EmployeeId,
                command.ServiceId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Service is already assigned to the employee.");
        }

        var assignment =
            EmployeeService.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.EmployeeId,
                command.ServiceId,
                _clock.UtcNow);

        await _employeeServiceRepository.AddAsync(
            assignment,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Map(assignment);
    }

    private static EmployeeServiceDetails Map(
        EmployeeService assignment)
    {
        return new EmployeeServiceDetails(
            assignment.Id,
            assignment.OrganizationId,
            assignment.EmployeeId,
            assignment.ServiceId,
            assignment.AssignedAtUtc);
    }
}
