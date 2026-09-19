using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Employees;

public sealed class EmployeeWorkingHours : Entity
{
    private EmployeeWorkingHours(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
        : base(id)
    {
        OrganizationId = organizationId;
        EmployeeId = employeeId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    public Guid OrganizationId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public static EmployeeWorkingHours Create(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            employeeId,
            nameof(employeeId));

        ValidateDayOfWeek(dayOfWeek);
        ValidateTimeRange(startTime, endTime);

        return new EmployeeWorkingHours(
            id,
            organizationId,
            employeeId,
            dayOfWeek,
            startTime,
            endTime);
    }

    public void UpdateHours(
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        ValidateDayOfWeek(dayOfWeek);
        ValidateTimeRange(startTime, endTime);

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    private static void ValidateRelatedId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Related entity id cannot be empty.",
                parameterName);
        }
    }

    private static void ValidateDayOfWeek(DayOfWeek dayOfWeek)
    {
        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dayOfWeek),
                dayOfWeek,
                "Unsupported day of week.");
        }
    }

    private static void ValidateTimeRange(
        TimeOnly startTime,
        TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "Working hours end time must be later than start time.",
                nameof(endTime));
        }
    }
}
