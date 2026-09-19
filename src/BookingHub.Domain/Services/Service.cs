using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Services;

public sealed class Service : AggregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int CurrencyCodeLength = 3;

    private static readonly TimeSpan MaxDuration =
        TimeSpan.FromHours(24);

    private Service(
        Guid id,
        Guid organizationId,
        string name,
        string? description,
        TimeSpan duration,
        decimal priceAmount,
        string currency,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        Duration = duration;
        PriceAmount = priceAmount;
        Currency = currency;
        Status = ServiceStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public TimeSpan Duration { get; private set; }

    public decimal PriceAmount { get; private set; }

    public string Currency { get; private set; }

    public ServiceStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Service Create(
        Guid id,
        Guid organizationId,
        string name,
        string? description,
        TimeSpan duration,
        decimal priceAmount,
        string currency,
        DateTimeOffset createdAtUtc)
    {
        ValidateOrganizationId(organizationId);
        ValidateName(name);
        ValidateDescription(description);
        ValidateDuration(duration);
        ValidatePrice(priceAmount);
        ValidateCurrency(currency);

        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new Service(
            id,
            organizationId,
            name.Trim(),
            NormalizeOptional(description),
            duration,
            priceAmount,
            NormalizeCurrency(currency),
            utcCreatedAt);
    }

    public void UpdateDetails(
        string name,
        string? description,
        TimeSpan duration,
        decimal priceAmount,
        string currency,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();
        ValidateName(name);
        ValidateDescription(description);
        ValidateDuration(duration);
        ValidatePrice(priceAmount);
        ValidateCurrency(currency);

        Name = name.Trim();
        Description = NormalizeOptional(description);
        Duration = duration;
        PriceAmount = priceAmount;
        Currency = NormalizeCurrency(currency);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        if (Status == ServiceStatus.Inactive)
        {
            return;
        }

        Status = ServiceStatus.Inactive;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (Status == ServiceStatus.Active)
        {
            return;
        }

        Status = ServiceStatus.Active;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    private void EnsureActive()
    {
        if (Status == ServiceStatus.Inactive)
        {
            throw new InvalidOperationException(
                "Inactive service cannot be modified.");
        }
    }

    private static void ValidateOrganizationId(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id cannot be empty.",
                nameof(organizationId));
        }
    }

    private static void ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Service name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }
    }

    private static void ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        if (description.Trim().Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Service description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }
    }

    private static void ValidateDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero ||
            duration > MaxDuration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Service duration must be greater than zero and no longer than 24 hours.");
        }
    }

    private static void ValidatePrice(decimal priceAmount)
    {
        if (priceAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priceAmount),
                priceAmount,
                "Service price cannot be negative.");
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
