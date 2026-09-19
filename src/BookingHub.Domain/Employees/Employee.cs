using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Employees;

public sealed class Employee : AggregateRoot
{
    public const int MaxFirstNameLength = 100;
    public const int MaxLastNameLength = 100;
    public const int MaxPositionLength = 150;

    private Employee(
        Guid id,
        Guid organizationId,
        Guid? userId,
        string firstName,
        string? lastName,
        string? position,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Position = position;
        Status = EmployeeStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid? UserId { get; private set; }

    public string FirstName { get; private set; }

    public string? LastName { get; private set; }

    public string? Position { get; private set; }

    public EmployeeStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Employee Create(
        Guid id,
        Guid organizationId,
        Guid? userId,
        string firstName,
        string? lastName,
        string? position,
        DateTimeOffset createdAtUtc)
    {
        ValidateOrganizationId(organizationId);
        ValidateUserId(userId);
        ValidateFirstName(firstName);
        ValidateOptionalValue(lastName, MaxLastNameLength, nameof(lastName));
        ValidateOptionalValue(position, MaxPositionLength, nameof(position));

        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new Employee(
            id,
            organizationId,
            userId,
            firstName.Trim(),
            NormalizeOptional(lastName),
            NormalizeOptional(position),
            utcCreatedAt);
    }

    public void UpdateProfile(
        string firstName,
        string? lastName,
        string? position,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();
        ValidateFirstName(firstName);
        ValidateOptionalValue(lastName, MaxLastNameLength, nameof(lastName));
        ValidateOptionalValue(position, MaxPositionLength, nameof(position));

        FirstName = firstName.Trim();
        LastName = NormalizeOptional(lastName);
        Position = NormalizeOptional(position);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void LinkUser(
        Guid userId,
        DateTimeOffset updatedAtUtc)
    {
        EnsureActive();
        ValidateUserId(userId);

        UserId = userId;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void UnlinkUser(DateTimeOffset updatedAtUtc)
    {
        EnsureActive();

        UserId = null;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return;
        }

        Status = EmployeeStatus.Inactive;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (Status == EmployeeStatus.Active)
        {
            return;
        }

        Status = EmployeeStatus.Active;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    private void EnsureActive()
    {
        if (Status == EmployeeStatus.Inactive)
        {
            throw new InvalidOperationException(
                "Inactive employee cannot be modified.");
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

    private static void ValidateUserId(Guid? userId)
    {
        if (userId.HasValue && userId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty when specified.",
                nameof(userId));
        }
    }

    private static void ValidateFirstName(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);

        if (firstName.Trim().Length > MaxFirstNameLength)
        {
            throw new ArgumentException(
                $"First name cannot exceed {MaxFirstNameLength} characters.",
                nameof(firstName));
        }
    }

    private static void ValidateOptionalValue(
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
