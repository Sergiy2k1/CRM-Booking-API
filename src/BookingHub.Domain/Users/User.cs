using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Users;

public sealed class User : AggregateRoot
{
    public const int MaxEmailLength = 320;
    public const int MaxNameLength = 100;
    public const int MaxPasswordHashLength = 1000;

    private User(
        Guid id,
        string email,
        string normalizedEmail,
        string passwordHash,
        string firstName,
        string lastName,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Email { get; private set; }

    public string NormalizedEmail { get; private set; }

    public string PasswordHash { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static User Create(
        Guid id,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        DateTimeOffset createdAtUtc)
    {
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash);
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        var normalizedEmail = NormalizeEmail(email);
        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new User(
            id,
            email.Trim(),
            normalizedEmail,
            passwordHash,
            firstName.Trim(),
            lastName.Trim(),
            utcCreatedAt);
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        DateTimeOffset updatedAtUtc)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangePasswordHash(
        string passwordHash,
        DateTimeOffset updatedAtUtc)
    {
        ValidatePasswordHash(passwordHash);

        PasswordHash = passwordHash;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    private static void ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (email.Trim().Length > MaxEmailLength)
        {
            throw new ArgumentException(
                $"Email cannot exceed {MaxEmailLength} characters.",
                nameof(email));
        }
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        if (passwordHash.Length > MaxPasswordHashLength)
        {
            throw new ArgumentException(
                $"Password hash cannot exceed {MaxPasswordHashLength} characters.",
                nameof(passwordHash));
        }
    }

    private static void ValidateName(
        string name,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            name,
            parameterName);

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Name cannot exceed {MaxNameLength} characters.",
                parameterName);
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email
            .Trim()
            .ToUpperInvariant();
    }
}
