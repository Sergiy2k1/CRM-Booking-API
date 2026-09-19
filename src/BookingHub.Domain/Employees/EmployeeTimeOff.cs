using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Employees;

public sealed class EmployeeTimeOff : AggregateRoot
{
    public const int MaxReasonLength = 500;

    private EmployeeTimeOff(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? reason,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        EmployeeId = employeeId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Reason = reason;
        Status = EmployeeTimeOffStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public DateTimeOffset StartsAtUtc { get; private set; }

    public DateTimeOffset EndsAtUtc { get; private set; }

    public string? Reason { get; private set; }

    public EmployeeTimeOffStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public static EmployeeTimeOff Create(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? reason,
        DateTimeOffset createdAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            employeeId,
            nameof(employeeId));

        var utcStartsAt = startsAtUtc.ToUniversalTime();
        var utcEndsAt = endsAtUtc.ToUniversalTime();

        ValidateTimeRange(
            utcStartsAt,
            utcEndsAt);

        ValidateReason(reason);

        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new EmployeeTimeOff(
            id,
            organizationId,
            employeeId,
            utcStartsAt,
            utcEndsAt,
            NormalizeOptional(reason),
            utcCreatedAt);
    }

    public void Reschedule(
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();

        var utcStartsAt = startsAtUtc.ToUniversalTime();
        var utcEndsAt = endsAtUtc.ToUniversalTime();

        ValidateTimeRange(
            utcStartsAt,
            utcEndsAt);

        StartsAtUtc = utcStartsAt;
        EndsAtUtc = utcEndsAt;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void UpdateReason(
        string? reason,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();
        ValidateReason(reason);

        Reason = NormalizeOptional(reason);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        if (Status == EmployeeTimeOffStatus.Cancelled)
        {
            return;
        }

        var utcCancelledAt = cancelledAtUtc.ToUniversalTime();

        Status = EmployeeTimeOffStatus.Cancelled;
        CancelledAtUtc = utcCancelledAt;
        UpdatedAtUtc = utcCancelledAt;
    }

    private void EnsureActive()
    {
        if (Status == EmployeeTimeOffStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Cancelled employee time off cannot be modified.");
        }
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

    private static void ValidateTimeRange(
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new ArgumentException(
                "Time off end must be later than start.",
                nameof(endsAtUtc));
        }
    }

    private static void ValidateReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return;
        }

        if (reason.Trim().Length > MaxReasonLength)
        {
            throw new ArgumentException(
                $"Time off reason cannot exceed {MaxReasonLength} characters.",
                nameof(reason));
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
