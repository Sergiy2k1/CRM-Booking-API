using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Organizations;

public sealed class Organization : AggregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxSlugLength = 100;
    public const int MaxTimeZoneLength = 100;

    private Organization(
        Guid id,
        string name,
        string slug,
        string timeZone,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = name;
        Slug = slug;
        TimeZone = timeZone;
        Status = OrganizationStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public string TimeZone { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Organization Create(
        Guid id,
        string name,
        string slug,
        string timeZone,
        DateTimeOffset createdAtUtc)
    {
        ValidateName(name);
        ValidateSlug(slug);
        ValidateTimeZone(timeZone);

        return new Organization(
            id,
            name.Trim(),
            NormalizeSlug(slug),
            timeZone.Trim(),
            createdAtUtc.ToUniversalTime());
    }

    public void Rename(
        string name,
        DateTimeOffset updatedAtUtc)
    {
        ValidateName(name);

        Name = name.Trim();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeTimeZone(
        string timeZone,
        DateTimeOffset updatedAtUtc)
    {
        ValidateTimeZone(timeZone);

        TimeZone = timeZone.Trim();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Suspend(DateTimeOffset updatedAtUtc)
    {
        if (Status == OrganizationStatus.Suspended)
        {
            return;
        }

        Status = OrganizationStatus.Suspended;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (Status == OrganizationStatus.Active)
        {
            return;
        }

        Status = OrganizationStatus.Active;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    private static void ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Organization name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }
    }

    private static void ValidateSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var normalizedSlug = NormalizeSlug(slug);

        if (normalizedSlug.Length > MaxSlugLength)
        {
            throw new ArgumentException(
                $"Organization slug cannot exceed {MaxSlugLength} characters.",
                nameof(slug));
        }

        foreach (var character in normalizedSlug)
        {
            var isLetter =
                character is >= 'a' and <= 'z';

            var isDigit =
                character is >= '0' and <= '9';

            if (!isLetter &&
                !isDigit &&
                character != '-')
            {
                throw new ArgumentException(
                    "Organization slug can contain only lowercase letters, numbers, and hyphens.",
                    nameof(slug));
            }
        }
    }

    private static void ValidateTimeZone(string timeZone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZone);

        if (timeZone.Trim().Length > MaxTimeZoneLength)
        {
            throw new ArgumentException(
                $"Time zone cannot exceed {MaxTimeZoneLength} characters.",
                nameof(timeZone));
        }
    }

    private static string NormalizeSlug(string slug)
    {
        return slug
            .Trim()
            .ToLowerInvariant();
    }
}