using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Customers;

public sealed class Customer : AggregateRoot
{
    public const int MaxFirstNameLength = 100;
    public const int MaxLastNameLength = 100;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneLength = 32;

    private Customer(
        Guid id,
        Guid organizationId,
        string firstName,
        string? lastName,
        string? email,
        string? phone,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Status = CustomerStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public string FirstName { get; private set; }

    public string? LastName { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public CustomerStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static Customer Create(
        Guid id,
        Guid organizationId,
        string firstName,
        string? lastName,
        string? email,
        string? phone,
        DateTimeOffset createdAtUtc)
    {
        ValidateOrganizationId(organizationId);
        ValidateRequiredName(firstName, nameof(firstName));
        ValidateOptionalName(lastName, nameof(lastName));
        ValidateOptionalContact(email, MaxEmailLength, nameof(email));
        ValidateOptionalContact(phone, MaxPhoneLength, nameof(phone));

        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new Customer(
            id,
            organizationId,
            firstName.Trim(),
            NormalizeOptional(lastName),
            NormalizeOptional(email),
            NormalizeOptional(phone),
            utcCreatedAt);
    }

    public void UpdateName(
        string firstName,
        string? lastName,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();
        ValidateRequiredName(firstName, nameof(firstName));
        ValidateOptionalName(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = NormalizeOptional(lastName);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void UpdateContactDetails(
        string? email,
        string? phone,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();
        ValidateOptionalContact(email, MaxEmailLength, nameof(email));
        ValidateOptionalContact(phone, MaxPhoneLength, nameof(phone));

        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Archive(DateTimeOffset archivedAtUtc)
    {
        if (Status == CustomerStatus.Archived)
        {
            return;
        }

        var utcArchivedAt = archivedAtUtc.ToUniversalTime();

        Status = CustomerStatus.Archived;
        ArchivedAtUtc = utcArchivedAt;
        UpdatedAtUtc = utcArchivedAt;
    }

    public void Restore(DateTimeOffset restoredAtUtc)
    {
        if (Status == CustomerStatus.Active)
        {
            return;
        }

        Status = CustomerStatus.Active;
        ArchivedAtUtc = null;
        UpdatedAtUtc = restoredAtUtc.ToUniversalTime();
    }

    private void EnsureActive()
    {
        if (Status == CustomerStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived customer cannot be modified.");
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

    private static void ValidateRequiredName(
        string name,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            name,
            parameterName);

        if (name.Trim().Length > MaxFirstNameLength)
        {
            throw new ArgumentException(
                $"Name cannot exceed {MaxFirstNameLength} characters.",
                parameterName);
        }
    }

    private static void ValidateOptionalName(
        string? name,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (name.Trim().Length > MaxLastNameLength)
        {
            throw new ArgumentException(
                $"Name cannot exceed {MaxLastNameLength} characters.",
                parameterName);
        }
    }

    private static void ValidateOptionalContact(
        string? value,
        int maxLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Trim().Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
