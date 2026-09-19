using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Services;

public sealed class EmployeeService : Entity
{
    private EmployeeService(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        DateTimeOffset assignedAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        EmployeeId = employeeId;
        ServiceId = serviceId;
        AssignedAtUtc = assignedAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public Guid ServiceId { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }

    public static EmployeeService Create(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        DateTimeOffset assignedAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            employeeId,
            nameof(employeeId));

        ValidateRelatedId(
            serviceId,
            nameof(serviceId));

        return new EmployeeService(
            id,
            organizationId,
            employeeId,
            serviceId,
            assignedAtUtc.ToUniversalTime());
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
}
