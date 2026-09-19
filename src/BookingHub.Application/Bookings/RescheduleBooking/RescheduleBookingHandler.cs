using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Bookings.Common;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Bookings.RescheduleBooking;

public sealed class RescheduleBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeScheduleRepository _employeeScheduleRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleBookingHandler(
        IBookingRepository bookingRepository,
        IOrganizationRepository organizationRepository,
        IEmployeeRepository employeeRepository,
        IEmployeeScheduleRepository employeeScheduleRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _organizationRepository = organizationRepository;
        _employeeRepository = employeeRepository;
        _employeeScheduleRepository = employeeScheduleRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDetails> HandleAsync(
        RescheduleBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var booking =
            await _bookingRepository.GetTrackedByOrganizationAndIdAsync(
                command.OrganizationId,
                command.BookingId,
                cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException(
                nameof(Booking),
                command.BookingId);
        }

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
                "Booking cannot be rescheduled for a suspended organization.");
        }

        var employee =
            await _employeeRepository.GetByOrganizationAndIdAsync(
                command.OrganizationId,
                booking.EmployeeId,
                cancellationToken);

        if (employee is null ||
            employee.Status != EmployeeStatus.Active)
        {
            throw new InvalidOperationException(
                "Booking cannot be rescheduled for an inactive employee.");
        }

        var duration =
            booking.EndsAtUtc - booking.StartsAtUtc;

        var startsAtUtc =
            command.StartsAtUtc.ToUniversalTime();

        var endsAtUtc =
            startsAtUtc.Add(duration);

        var workingHours =
            await _employeeScheduleRepository.GetWorkingHoursAsync(
                command.OrganizationId,
                booking.EmployeeId,
                cancellationToken);

        var timeOff =
            await _employeeScheduleRepository.GetTimeOffAsync(
                command.OrganizationId,
                booking.EmployeeId,
                startsAtUtc,
                endsAtUtc,
                cancellationToken);

        var overlappingBookings =
            await _bookingRepository.GetOverlappingAsync(
                command.OrganizationId,
                booking.EmployeeId,
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
                booking.EmployeeId,
                timeZone,
                startsAtUtc,
                endsAtUtc,
                booking.Id);

        if (availabilityStatus != EmployeeAvailabilityStatus.Available)
        {
            throw new BookingUnavailableException(
                availabilityStatus);
        }

        booking.Reschedule(
            startsAtUtc,
            endsAtUtc,
            _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return booking.ToDetails();
    }
}
