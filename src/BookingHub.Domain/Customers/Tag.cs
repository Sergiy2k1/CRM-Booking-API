using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Customers;

public sealed class Tag : AggregateRoot
{
    public const int MaxNameLength = 100;

    private Tag(
        Guid id,
        Guid organizationId,
        string name,
        string normalizedName,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        NormalizedName = normalizedName;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; }

    public string NormalizedName { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Tag Create(
        Guid id,
        Guid organizationId,
        string name,
        DateTimeOffset createdAtUtc)
    {
        ValidateOrganizationId(organizationId);
        ValidateName(name);

        var trimmedName = name.Trim();
        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new Tag(
            id,
            organizationId,
            trimmedName,
            NormalizeName(trimmedName),
            utcCreatedAt);
    }

    public void Rename(
        string name,
        DateTimeOffset updatedAtUtc)
    {
        ValidateName(name);

        var trimmedName = name.Trim();

        Name = trimmedName;
        NormalizedName = NormalizeName(trimmedName);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
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
                $"Tag name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }
    }

    private static string NormalizeName(string name)
    {
        return name.ToUpperInvariant();
    }
}
