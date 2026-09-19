using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Application.Employees.Common;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Employees.CreateEmployee;

public sealed class CreateEmployeeHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeHandler(
        IOrganizationRepository organizationRepository,
        IEmployeeRepository employeeRepository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _employeeRepository = employeeRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDetails> HandleAsync(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateOrganizationId(command.OrganizationId);

        var organization =
            await _organizationRepository.GetByIdAsync(
                command.OrganizationId,
                cancellationToken);

        if (organization is null)
        {
            throw new EntityNotFoundException(
                nameof(Organization),
                command.OrganizationId);
        }

        if (organization.Status != OrganizationStatus.Active)
        {
            throw new InvalidOperationException(
                "Employee cannot be created for a suspended organization.");
        }

        var employee =
            Employee.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                null,
                command.FirstName,
                command.LastName,
                command.Position,
                _clock.UtcNow);

        await _employeeRepository.AddAsync(
            employee,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return employee.ToDetails();
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
}
