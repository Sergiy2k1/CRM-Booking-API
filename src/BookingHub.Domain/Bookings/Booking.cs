using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Bookings;

public sealed class Booking : AggregateRoot
{
    public const int CurrencyCodeLength = 3;
    public const int MaxNotesLength = 2000;

    private Booking(
        Guid id,
        Guid organizationId,
        Guid customerId,
        Guid employeeId,
        Guid serviceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        decimal priceAmount,
        string currency,
        string? notes,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        CustomerId = customerId;
        EmployeeId = employeeId;
        ServiceId = serviceId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        PriceAmount = priceAmount;
        Currency = currency;
        Notes = notes;
        Status = BookingStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public Guid ServiceId { get; private set; }

    public DateTimeOffset StartsAtUtc { get; private set; }

    public DateTimeOffset EndsAtUtc { get; private set; }

    public decimal PriceAmount { get; private set; }

    public string Currency { get; private set; }

    public string? Notes { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? NoShowAtUtc { get; private set; }

    public static Booking Create(
        Guid id,
        Guid organizationId,
        Guid customerId,
        Guid employeeId,
        Guid serviceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        decimal priceAmount,
        string currency,
        string? notes,
        DateTimeOffset createdAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            customerId,
            nameof(customerId));

        ValidateRelatedId(
            employeeId,
            nameof(employeeId));

        ValidateRelatedId(
            serviceId,
            nameof(serviceId));

        var utcStartsAt = startsAtUtc.ToUniversalTime();
        var utcEndsAt = endsAtUtc.ToUniversalTime();

        ValidateTimeRange(
            utcStartsAt,
            utcEndsAt);

        ValidatePrice(priceAmount);
        ValidateCurrency(currency);
        ValidateNotes(notes);

        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new Booking(
            id,
            organizationId,
            customerId,
            employeeId,
            serviceId,
            utcStartsAt,
            utcEndsAt,
            priceAmount,
            NormalizeCurrency(currency),
            NormalizeOptional(notes),
            utcCreatedAt);
    }

    public void Confirm(DateTimeOffset confirmedAtUtc)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending booking can be confirmed.");
        }

        Status = BookingStatus.Confirmed;
        UpdatedAtUtc = confirmedAtUtc.ToUniversalTime();
    }

    public void Reschedule(
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        EnsureCanModifySchedule();

        var utcStartsAt = startsAtUtc.ToUniversalTime();
        var utcEndsAt = endsAtUtc.ToUniversalTime();

        ValidateTimeRange(
            utcStartsAt,
            utcEndsAt);

        StartsAtUtc = utcStartsAt;
        EndsAtUtc = utcEndsAt;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void UpdateNotes(
        string? notes,
        DateTimeOffset updatedAtUtc)
    {
        EnsureNotTerminal();
        ValidateNotes(notes);

        Notes = NormalizeOptional(notes);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        if (Status is BookingStatus.Completed or
            BookingStatus.Cancelled or
            BookingStatus.NoShow)
        {
            throw new InvalidOperationException(
                "Completed, cancelled, or no-show booking cannot be cancelled.");
        }

        var utcCancelledAt = cancelledAtUtc.ToUniversalTime();

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = utcCancelledAt;
        UpdatedAtUtc = utcCancelledAt;
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Only confirmed booking can be completed.");
        }

        var utcCompletedAt = completedAtUtc.ToUniversalTime();

        Status = BookingStatus.Completed;
        CompletedAtUtc = utcCompletedAt;
        UpdatedAtUtc = utcCompletedAt;
    }

    public void MarkNoShow(DateTimeOffset noShowAtUtc)
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Only confirmed booking can be marked as no-show.");
        }

        var utcNoShowAt = noShowAtUtc.ToUniversalTime();

        Status = BookingStatus.NoShow;
        NoShowAtUtc = utcNoShowAt;
        UpdatedAtUtc = utcNoShowAt;
    }

    private void EnsureCanModifySchedule()
    {
        if (Status is not BookingStatus.Pending and
            not BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Only pending or confirmed booking can be rescheduled.");
        }
    }

    private void EnsureNotTerminal()
    {
        if (Status is BookingStatus.Completed or
            BookingStatus.Cancelled or
            BookingStatus.NoShow)
        {
            throw new InvalidOperationException(
                "Terminal booking cannot be modified.");
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
                "Booking end must be later than start.",
                nameof(endsAtUtc));
        }
    }

    private static void ValidatePrice(decimal priceAmount)
    {
        if (priceAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priceAmount),
                priceAmount,
                "Booking price cannot be negative.");
        }
    }

    private static void ValidateCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var normalizedCurrency = NormalizeCurrency(currency);

        if (normalizedCurrency.Length != CurrencyCodeLength)
        {
            throw new ArgumentException(
                $"Currency must contain exactly {CurrencyCodeLength} letters.",
                nameof(currency));
        }

        foreach (var character in normalizedCurrency)
        {
            if (character is < 'A' or > 'Z')
            {
                throw new ArgumentException(
                    "Currency must contain only Latin letters.",
                    nameof(currency));
            }
        }
    }

    private static void ValidateNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return;
        }

        if (notes.Trim().Length > MaxNotesLength)
        {
            throw new ArgumentException(
                $"Booking notes cannot exceed {MaxNotesLength} characters.",
                nameof(notes));
        }
    }

    private static string NormalizeCurrency(string currency)
    {
        return currency
            .Trim()
            .ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
