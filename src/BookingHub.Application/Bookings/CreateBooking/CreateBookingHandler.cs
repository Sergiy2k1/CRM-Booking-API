using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;

namespace BookingHub.Application.Bookings.CreateBooking;

public sealed class CreateBookingHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IEmployeeServiceRepository _employeeServiceRepository;
    private readonly IEmployeeScheduleRepository _employeeScheduleRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public CreateBookingHandler(
        IOrganizationRepository organizationRepository,
        ICustomerRepository customerRepository,
        IEmployeeRepository employeeRepository,
        IServiceRepository serviceRepository,
        IEmployeeServiceRepository employeeServiceRepository,
        IEmployeeScheduleRepository employeeScheduleRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IGuidGenerator guidGenerator)
    {
        _organizationRepository = organizationRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _serviceRepository = serviceRepository;
        _employeeServiceRepository = employeeServiceRepository;
        _employeeScheduleRepository = employeeScheduleRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _guidGenerator = guidGenerator;
    }

    public async Task<CreateBookingResult> HandleAsync(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidateId(
            command.OrganizationId,
            nameof(command.OrganizationId));

        ValidateId(
            command.CustomerId,
            nameof(command.CustomerId));

        ValidateId(
            command.EmployeeId,
            nameof(command.EmployeeId));

        ValidateId(
            command.ServiceId,
            nameof(command.ServiceId));

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
                "Booking cannot be created for a suspended organization.");
        }

        var customer =
            await _customerRepository.GetByIdAsync(
                command.CustomerId,
                cancellationToken);

        if (customer is null ||
            customer.OrganizationId != command.OrganizationId)
        {
            throw new EntityNotFoundException(
                nameof(Customer),
                command.CustomerId);
        }

        if (customer.Status != CustomerStatus.Active)
        {
            throw new InvalidOperationException(
                "Booking cannot be created for an archived customer.");
        }

        var employee =
            await _employeeRepository.GetByIdAsync(
                command.EmployeeId,
                cancellationToken);

        if (employee is null ||
            employee.OrganizationId != command.OrganizationId)
        {
            throw new EntityNotFoundException(
                nameof(Employee),
                command.EmployeeId);
        }

        if (employee.Status != EmployeeStatus.Active)
        {
            throw new InvalidOperationException(
                "Booking cannot be created for an inactive employee.");
        }

        var service =
            await _serviceRepository.GetByIdAsync(
                command.ServiceId,
                cancellationToken);

        if (service is null ||
            service.OrganizationId != command.OrganizationId)
        {
            throw new EntityNotFoundException(
                nameof(Service),
                command.ServiceId);
        }

        if (service.Status != ServiceStatus.Active)
        {
            throw new InvalidOperationException(
                "Booking cannot be created for an inactive service.");
        }

        var isServiceAssigned =
            await _employeeServiceRepository.IsAssignedAsync(
                command.OrganizationId,
                command.EmployeeId,
                command.ServiceId,
                cancellationToken);

        if (!isServiceAssigned)
        {
            throw new InvalidOperationException(
                "Selected employee does not provide the selected service.");
        }

        var startsAtUtc =
            command.StartsAtUtc.ToUniversalTime();

        var endsAtUtc =
            startsAtUtc.Add(service.Duration);

        var workingHours =
            await _employeeScheduleRepository.GetWorkingHoursAsync(
                command.OrganizationId,
                command.EmployeeId,
                cancellationToken);

        var timeOff =
            await _employeeScheduleRepository.GetTimeOffAsync(
                command.OrganizationId,
                command.EmployeeId,
                startsAtUtc,
                endsAtUtc,
                cancellationToken);

        var overlappingBookings =
            await _bookingRepository.GetOverlappingAsync(
                command.OrganizationId,
                command.EmployeeId,
                startsAtUtc,
                endsAtUtc,
                cancellationToken);

        var timeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                organization.TimeZone);

        var availabilityStatus =
            EmployeeAvailabilityService.Check(
                workingHours,
                timeOff,
                overlappingBookings,
                command.OrganizationId,
                command.EmployeeId,
                timeZone,
                startsAtUtc,
                endsAtUtc);

        if (availabilityStatus != EmployeeAvailabilityStatus.Available)
        {
            throw new BookingUnavailableException(
                availabilityStatus);
        }

        var booking =
            Booking.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.CustomerId,
                command.EmployeeId,
                command.ServiceId,
                startsAtUtc,
                endsAtUtc,
                service.PriceAmount,
                service.Currency,
                command.Notes,
                _clock.UtcNow);

        await _bookingRepository.AddAsync(
            booking,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateBookingResult(
            booking.Id,
            booking.Status,
            booking.StartsAtUtc,
            booking.EndsAtUtc,
            booking.PriceAmount,
            booking.Currency);
    }

    private static void ValidateId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                parameterName);
        }
    }
}
